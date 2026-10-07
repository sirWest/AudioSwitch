/*
  LICENSE
  -------
  Copyright (C) 2007-2010 Ray Molenkamp

  This source code is provided 'as-is', without any express or implied
  warranty.  In no event will the authors be held liable for any damages
  arising from the use of this source code or the software it produces.

  Permission is granted to anyone to use this source code for any purpose,
  including commercial applications, and to alter it and redistribute it
  freely, subject to the following restrictions:

  1. The origin of this source code must not be misrepresented; you must not
     claim that you wrote the original source code.  If you use this source code
     in a product, an acknowledgment in the product documentation would be
     appreciated but is not required.
  2. Altered source versions must be plainly marked as such, and must not be
     misrepresented as being the original source code.
  3. This notice may not be removed or altered from any source distribution.
*/
// Modified for AudioSwitch: focused wrappers with deterministic ownership,
// explicit settings-independent properties, and safe notification teardown.
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AudioSwitch.Core.Audio;

namespace AudioSwitch.Core.CoreAudio;

internal sealed class MMDeviceEnumerator : IDisposable
{
    internal MMDeviceEnumerator() { }

    internal MMDeviceEnumerator(IMMDeviceEnumerator native) => this.native = native;

    private readonly ConcurrentDictionary<string, DeviceSession> sessions = new();

    internal void Invalidate(string? id)
    {
        if (id is null)
        {
            foreach (var session in sessions.Values)
                session.Reset();
        }
        else if (sessions.TryGetValue(id, out var session))
            session.Reset();
    }

    private MMDevice Wrap(IMMDevice nativeDevice, string? knownId = null, DataFlow? flow = null)
    {
        var device = new MMDevice(nativeDevice);
        try
        {
            var id = knownId ?? device.ID;
            device.Session = sessions.GetOrAdd(id, key => new DeviceSession(key));
            device.Session.Note("id", id);
            if (flow is not null)
                device.Session.Note("direction", flow.ToString()!);
            return device;
        }
        catch
        {
            device.Dispose();
            throw;
        }
    }

    private IMMDeviceEnumerator? native;
    private bool disposed;
    private IMMDeviceEnumerator Native
    {
        get
        {
            ObjectDisposedException.ThrowIf(disposed, this);
            try
            {
                return native ??= (IMMDeviceEnumerator)new EnumeratorObject();
            }
            catch (Exception ex)
            {
                throw new AudioOperationException("create audio enumerator", ex);
            }
        }
    }

    private IMMNotificationClient? callback;

    internal MMDevice GetDefaultAudioEndpoint(DataFlow flow, Role role)
    {
        AudioOperationException.Check(
            Native.GetDefaultAudioEndpoint(flow, role, out var device),
            $"get default {flow}/{role}"
        );
        return Wrap(device, flow: flow);
    }

    internal MMDevice GetDevice(string id)
    {
        AudioOperationException.Check(Native.GetDevice(id, out var device), $"get endpoint '{id}'");
        var wrapped = Wrap(device, id);
        try
        {
            wrapped.Session.Note("name", wrapped.FriendlyName);
            wrapped.Session.Note("description", wrapped.DeviceFriendlyName);
            wrapped.Session.Note("icon", wrapped.IconPath);
            return wrapped;
        }
        catch
        {
            wrapped.Dispose();
            throw;
        }
    }

    // The caller owns each yielded device; the iterator owns the collection.
    internal IEnumerable<MMDevice> EnumerateAudioEndPoints(DataFlow flow, DeviceState state)
    {
        var result = Native.EnumAudioEndpoints(flow, state, out var collection);
        AudioOperationException.Check(result);
        try
        {
            result = collection.GetCount(out var count);
            AudioOperationException.Check(result);
            for (uint i = 0; i < count; i++)
            {
                result = collection.Item(i, out var device);
                if (result < 0)
                {
                    AudioDiagnostics.Log.Failure(
                        $"enumerate {flow} endpoint index {i}",
                        new AudioOperationException(
                            "read collection item",
                            Marshal.GetExceptionForHR(result, -1)!
                        )
                    );
                    continue;
                }

                MMDevice wrapped;
                try
                {
                    wrapped = Wrap(device, flow: flow);
                }
                catch (Exception ex) when (AudioOperationException.IsDeviceFailure(ex))
                {
                    AudioDiagnostics.Log.Failure($"identify {flow} endpoint index {i}", ex);
                    continue;
                }
                yield return wrapped;
            }
        }
        finally
        {
            if (Marshal.IsComObject(collection))
            {
                MMDevice.Release(collection);
            }
        }
    }

    internal void RegisterEndpointNotificationCallback(IMMNotificationClient client)
    {
        if (callback is not null)
        {
            throw new InvalidOperationException("A callback is already registered.");
        }

        AudioOperationException.Check(Native.RegisterEndpointNotificationCallback(client));
        callback = client;
    }

    public void Dispose()
    {
        disposed = true;
        var instance = Interlocked.Exchange(ref native, null);
        if (instance is null)
        {
            return;
        }

        try
        {
            if (callback is not null)
            {
                var hr = instance.UnregisterEndpointNotificationCallback(callback);
                if (hr < 0)
                {
                    AudioDiagnostics.Log.Failure(
                        "unregister endpoint callback",
                        new COMException("Endpoint callback cleanup failed", hr)
                    );
                }

                callback = null;
            }
        }
        catch (Exception ex) when (AudioOperationException.IsDeviceFailure(ex))
        {
            AudioDiagnostics.Log.Failure("unregister endpoint callback", ex);
        }
        finally
        {
            if (Marshal.IsComObject(instance))
            {
                MMDevice.Release(instance);
            }
        }
    }
}

internal sealed class MMDevice(IMMDevice native) : IDisposable
{
    internal DeviceSession Session { get; set; } = new();
    private IMMDevice? native = native;

    private void Check(int result, [CallerMemberName] string operation = "")
    {
        if (result >= 0)
            return;
        var error = new AudioOperationException(operation, Marshal.GetExceptionForHR(result, -1)!);
        Session.Failed(operation, error);
        throw error;
    }

    private IMMDevice Native => native ?? throw new ObjectDisposedException(nameof(MMDevice));

    private AudioEndpointVolume? volume;
    private AudioMeterInformation? meter;
    internal string ID
    {
        get
        {
            Check(Native.GetId(out var id), "read endpoint ID");
            Session.Note("id", id);
            return id;
        }
    }

    internal DeviceState State
    {
        get
        {
            Check(Native.GetState(out var state), "read endpoint state");
            Session.Note("state", state.ToString());
            return state;
        }
    }

    internal string FriendlyName =>
        Property(new(new Guid("A45C254E-DF1C-4EFD-8020-67D146A850E0"), 14)) ?? "<Unknown name>";
    internal string DeviceFriendlyName =>
        Property(new(new Guid("A45C254E-DF1C-4EFD-8020-67D146A850E0"), 2)) ?? FriendlyName;
    internal string IconPath =>
        Property(new(new Guid("259ABFFC-50A7-47CE-AF08-68C9A7D73366"), 12)) ?? "";
    internal AudioEndpointVolume AudioEndpointVolume =>
        volume ??= new(Activate<IAudioEndpointVolume>(), Session);
    internal AudioMeterInformation AudioMeterInformation =>
        meter ??= new(Activate<IAudioMeterInformation>(), Session);

    internal void ResetMeter()
    {
        var previous = meter;
        meter = null;
        previous?.Dispose();
    }

    private T Activate<T>()
    {
        var operation = "activate " + typeof(T).Name;
        if (Session.Blocked(operation) is { } previous)
            throw previous;
        try
        {
            var iid = typeof(T).GUID;
            Check(Native.Activate(ref iid, 23, 0, out var result), operation);
            try
            {
                var value = (T)result;
                Session.Succeeded(operation);
                return value;
            }
            catch
            {
                Release(result);
                throw;
            }
        }
        catch (Exception ex) when (AudioOperationException.IsDeviceFailure(ex))
        {
            Session.Failed(operation, ex);
            throw;
        }
    }

    private string? Property(PropertyKey key)
    {
        var operation = $"property {key.Format}/{key.Id}";
        if (Session.TryProperty(operation, out var cached))
            return cached;
        if (Session.Blocked(operation) is not null)
            return null;
        IPropertyStore? store = null;
        var value = new PropVariant();
        try
        {
            Check(Native.OpenPropertyStore(0, out store), operation);
            Check(store.GetValue(ref key, out value), operation);
            var text = value.String;
            text = string.IsNullOrWhiteSpace(text) ? null : text;
            Session.Property(operation, text);
            return text;
        }
        catch (Exception ex) when (AudioOperationException.IsDeviceFailure(ex))
        {
            Session.Failed(operation, ex);
            return null;
        }
        finally
        {
            var result = PropVariant.PropVariantClear(ref value);
            if (result < 0)
                AudioDiagnostics.Log.Failure(
                    "clear property value",
                    new COMException("Property cleanup failed", result),
                    Session.Id,
                    Session.Snapshot
                );
            Release(store);
        }
    }

    internal static void Release(object? instance)
    {
        try
        {
            if (instance is not null && Marshal.IsComObject(instance))
                Marshal.ReleaseComObject(instance);
        }
        catch (Exception ex)
        {
            AudioDiagnostics.Log.Failure("release COM object", ex);
        }
    }

    public void Dispose()
    {
        var instance = Interlocked.Exchange(ref native, null);
        if (instance is null)
        {
            return;
        }

        try
        {
            volume?.Dispose();
        }
        finally
        {
            try
            {
                meter?.Dispose();
            }
            finally
            {
                if (Marshal.IsComObject(instance))
                {
                    MMDevice.Release(instance);
                }
            }
        }
    }
}

internal sealed class AudioEndpointVolume(
    IAudioEndpointVolume native,
    DeviceSession? session = null
) : IDisposable
{
    private readonly DeviceSession session = session ?? new();

    private void Check(int result, [CallerMemberName] string operation = "")
    {
        if (result >= 0)
            return;
        var error = new AudioOperationException(operation, Marshal.GetExceptionForHR(result, -1)!);
        AudioDiagnostics.Log.Failure(operation, error, session.Id, session.Snapshot);
        throw error;
    }

    private IAudioEndpointVolume? native = native;
    private IAudioEndpointVolume Native =>
        native ?? throw new ObjectDisposedException(nameof(AudioEndpointVolume));

    private VolumeCallback? callback;
    internal float MasterVolumeLevelScalar
    {
        get
        {
            Check(Native.GetMasterVolumeLevelScalar(out var value), "read volume scalar");
            if (!float.IsFinite(value))
            {
                throw new AudioOperationException(
                    "read volume",
                    new InvalidDataException("The driver returned a non-finite volume.")
                );
            }
            return Math.Clamp(value, 0, 1);
        }
        set
        {
            var context = Guid.Empty;
            session.Note(
                "requestedVolume",
                value.ToString("R", System.Globalization.CultureInfo.InvariantCulture)
            );
            Check(Native.SetMasterVolumeLevelScalar(value, ref context), "write volume scalar");
        }
    }

    internal bool Mute
    {
        get
        {
            Check(Native.GetMute(out var value), "read mute");
            return value;
        }
        set
        {
            var context = Guid.Empty;
            session.Note("requestedMute", value.ToString());
            Check(Native.SetMute(value, ref context), "write mute");
        }
    }

    internal void Subscribe(Action<AudioState> handler)
    {
        if (callback is not null)
        {
            throw new InvalidOperationException("A callback is already registered.");
        }

        var candidate = new VolumeCallback(handler);
        Check(Native.RegisterControlChangeNotify(candidate), "subscribe volume notifications");
        callback = candidate;
    }

    public void Dispose()
    {
        var instance = Interlocked.Exchange(ref native, null);
        if (instance is null)
        {
            return;
        }

        try
        {
            if (callback is not null)
            {
                callback.Stop();
                var previous = callback;
                callback = null;
                var hr = instance.UnregisterControlChangeNotify(previous);
                if (hr < 0)
                {
                    AudioDiagnostics.Log.Failure(
                        "unregister volume callback",
                        new COMException("Volume callback cleanup failed", hr),
                        session.Id,
                        session.Snapshot
                    );
                }
            }
        }
        catch (Exception ex) when (AudioOperationException.IsDeviceFailure(ex))
        {
            AudioDiagnostics.Log.Failure(
                "unregister volume callback",
                ex,
                session.Id,
                session.Snapshot
            );
        }
        finally
        {
            if (Marshal.IsComObject(instance))
            {
                MMDevice.Release(instance);
            }
        }
    }
}

internal sealed class AudioMeterInformation(
    IAudioMeterInformation native,
    DeviceSession? session = null
) : IDisposable
{
    private readonly DeviceSession session = session ?? new();

    private void Check(int result, string operation)
    {
        if (result >= 0)
            return;
        var error = new AudioOperationException(operation, Marshal.GetExceptionForHR(result, -1)!);
        AudioDiagnostics.Log.Failure(operation, error, session.Id, session.Snapshot);
        throw error;
    }

    private IAudioMeterInformation? native = native;
    internal IReadOnlyList<float> PeakValues
    {
        get
        {
            var instance =
                native ?? throw new ObjectDisposedException(nameof(AudioMeterInformation));
            Check(instance.GetMeteringChannelCount(out var count), "read meter channel count");
            if (count == 0)
            {
                return Array.Empty<float>();
            }

            // Bound allocation even if a malfunctioning driver returns a corrupt count.
            if (count > 256)
            {
                throw new AudioOperationException(
                    "read peak levels",
                    new InvalidDataException($"Invalid metering channel count: {count}.")
                );
            }

            var peaks = new float[count];
            Check(instance.GetChannelsPeakValues(count, peaks), $"read {count} meter peaks");
            for (var i = 0; i < peaks.Length; i++)
            {
                peaks[i] = float.IsFinite(peaks[i]) ? Math.Clamp(peaks[i], 0, 1) : 0;
            }
            return peaks;
        }
    }

    public void Dispose()
    {
        var instance = Interlocked.Exchange(ref native, null);
        if (instance is not null && Marshal.IsComObject(instance))
        {
            MMDevice.Release(instance);
        }
    }
}

[StructLayout(LayoutKind.Sequential)]
internal struct VolumeNotification
{
    public Guid Context;
    public int Muted;
    public float Volume;
    public uint Channels;
}

[ComVisible(true), ClassInterface(ClassInterfaceType.None)]
public sealed class VolumeCallback : IAudioEndpointVolumeCallback
{
    private Action<AudioState>? handler;

    internal VolumeCallback(Action<AudioState> handler) => this.handler = handler;

    internal void Stop() => Interlocked.Exchange(ref handler, null);

    public int OnNotify(nint data)
    {
        if (data == 0)
        {
            return unchecked((int)0x80004003);
        }

        try
        {
            var value = Marshal.PtrToStructure<VolumeNotification>(data);
            if (float.IsFinite(value.Volume))
            {
                Volatile
                    .Read(ref handler)
                    ?.Invoke(new(Math.Clamp(value.Volume, 0, 1), value.Muted != 0));
            }
            return 0;
        }
        catch (Exception ex)
        {
            AudioDiagnostics.Log.Failure("audio notification callback", ex);
            return 0;
        }
    }
}

[ComVisible(true), ClassInterface(ClassInterfaceType.None)]
public sealed class EndpointNotifications : IMMNotificationClient
{
    private Action<AudioChange>? handler;

    internal EndpointNotifications(Action<AudioChange> handler) => this.handler = handler;

    internal void Stop() => Interlocked.Exchange(ref handler, null);

    private int Notify(AudioChange change)
    {
        // Never let subscriber exceptions escape into Windows' notification thread.
        try
        {
            Volatile.Read(ref handler)?.Invoke(change);
        }
        catch (Exception ex)
        {
            AudioDiagnostics.Log.Failure("audio notification callback", ex);
        }

        return 0;
    }

    public int OnDeviceStateChanged(string id, DeviceState state) =>
        Notify(new(id) { RefreshCapabilities = true });

    public int OnDeviceAdded(string id) => Notify(new(id) { RefreshCapabilities = true });

    public int OnDeviceRemoved(string id) => Notify(new(id) { RefreshCapabilities = true });

    public int OnDefaultDeviceChanged(DataFlow flow, Role role, string? id) =>
        Notify(
            new(
                id,
                flow == DataFlow.Render ? Direction.Playback
                    : flow == DataFlow.Capture ? Direction.Recording
                    : null,
                (AudioRole)role
            )
        );

    public int OnPropertyValueChanged(string id, PropertyKey key) =>
        Notify(new(id) { RefreshCapabilities = true });
}
