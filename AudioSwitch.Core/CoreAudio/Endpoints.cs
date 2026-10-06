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
using System.Diagnostics;
using System.Runtime.InteropServices;
using AudioSwitch.Core.Audio;

namespace AudioSwitch.Core.CoreAudio;

internal sealed class MMDeviceEnumerator : IDisposable
{
    internal MMDeviceEnumerator() { }

    internal MMDeviceEnumerator(IMMDeviceEnumerator native) => this.native = native;

    private IMMDeviceEnumerator? native;
    private bool disposed;
    private IMMDeviceEnumerator Native
    {
        get
        {
            ObjectDisposedException.ThrowIf(disposed, this);
            return native ??= (IMMDeviceEnumerator)new EnumeratorObject();
        }
    }

    private IMMNotificationClient? callback;

    internal MMDevice GetDefaultAudioEndpoint(DataFlow flow, Role role)
    {
        AudioOperationException.Check(Native.GetDefaultAudioEndpoint(flow, role, out var device));
        return new(device);
    }

    internal MMDevice GetDevice(string id)
    {
        AudioOperationException.Check(Native.GetDevice(id, out var device));
        return new(device);
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
                    Trace.TraceWarning($"Skipping audio endpoint {i}: 0x{result:X8}");
                    continue;
                }

                yield return new(device);
            }
        }
        finally
        {
            if (Marshal.IsComObject(collection))
            {
                Marshal.ReleaseComObject(collection);
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
                    Trace.TraceError($"Endpoint callback cleanup failed: 0x{hr:X8}");
                }

                callback = null;
            }
        }
        catch (Exception ex) when (AudioOperationException.IsDeviceFailure(ex))
        {
            Trace.TraceWarning($"Endpoint callback cleanup failed: {ex.Message}");
        }
        finally
        {
            if (Marshal.IsComObject(instance))
            {
                Marshal.ReleaseComObject(instance);
            }
        }
    }
}

internal sealed class MMDevice(IMMDevice native) : IDisposable
{
    private IMMDevice? native = native;
    private IMMDevice Native => native ?? throw new ObjectDisposedException(nameof(MMDevice));

    private AudioEndpointVolume? volume;
    private AudioMeterInformation? meter;
    internal string ID
    {
        get
        {
            AudioOperationException.Check(Native.GetId(out var id));
            return id;
        }
    }

    internal DeviceState State
    {
        get
        {
            AudioOperationException.Check(Native.GetState(out var state));
            return state;
        }
    }

    internal string FriendlyName =>
        Property(new(new Guid("A45C254E-DF1C-4EFD-8020-67D146A850E0"), 14)) ?? "Unknown";
    internal string DeviceFriendlyName =>
        Property(new(new Guid("A45C254E-DF1C-4EFD-8020-67D146A850E0"), 2)) ?? FriendlyName;
    internal string IconPath =>
        Property(new(new Guid("259ABFFC-50A7-47CE-AF08-68C9A7D73366"), 12)) ?? "";
    internal AudioEndpointVolume AudioEndpointVolume =>
        volume ??= new(Activate<IAudioEndpointVolume>());
    internal AudioMeterInformation AudioMeterInformation =>
        meter ??= new(Activate<IAudioMeterInformation>());

    internal void ResetMeter()
    {
        var previous = meter;
        meter = null;
        previous?.Dispose();
    }

    private T Activate<T>()
    {
        var iid = typeof(T).GUID;
        AudioOperationException.Check(Native.Activate(ref iid, 23, 0, out var result));
        try
        {
            return (T)result;
        }
        catch
        {
            if (Marshal.IsComObject(result))
            {
                Marshal.ReleaseComObject(result);
            }
            throw;
        }
    }

    private string? Property(PropertyKey key)
    {
        IPropertyStore? store = null;
        var value = new PropVariant();
        try
        {
            AudioOperationException.Check(Native.OpenPropertyStore(0, out store));
            AudioOperationException.Check(store.GetValue(ref key, out value));
            var text = value.String;
            return string.IsNullOrWhiteSpace(text) ? null : text;
        }
        catch (Exception ex) when (AudioOperationException.IsDeviceFailure(ex))
        {
            Trace.TraceWarning(
                $"Audio device property {key.Format}/{key.Id} unavailable: {ex.Message}"
            );
            return null;
        }
        finally
        {
            PropVariant.PropVariantClear(ref value);
            if (store is not null && Marshal.IsComObject(store))
            {
                Marshal.ReleaseComObject(store);
            }
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
                    Marshal.ReleaseComObject(instance);
                }
            }
        }
    }
}

internal sealed class AudioEndpointVolume(IAudioEndpointVolume native) : IDisposable
{
    private IAudioEndpointVolume? native = native;
    private IAudioEndpointVolume Native =>
        native ?? throw new ObjectDisposedException(nameof(AudioEndpointVolume));

    private VolumeCallback? callback;
    internal float MasterVolumeLevelScalar
    {
        get
        {
            AudioOperationException.Check(Native.GetMasterVolumeLevelScalar(out var value));
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
            AudioOperationException.Check(Native.SetMasterVolumeLevelScalar(value, ref context));
        }
    }

    internal bool Mute
    {
        get
        {
            AudioOperationException.Check(Native.GetMute(out var value));
            return value;
        }
        set
        {
            var context = Guid.Empty;
            AudioOperationException.Check(Native.SetMute(value, ref context));
        }
    }

    internal void Subscribe(Action<AudioState> handler)
    {
        if (callback is not null)
        {
            throw new InvalidOperationException("A callback is already registered.");
        }

        var candidate = new VolumeCallback(handler);
        AudioOperationException.Check(Native.RegisterControlChangeNotify(candidate));
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
                    Trace.TraceError($"Volume callback cleanup failed: 0x{hr:X8}");
                }
            }
        }
        catch (Exception ex) when (AudioOperationException.IsDeviceFailure(ex))
        {
            Trace.TraceWarning($"Volume callback cleanup failed: {ex.Message}");
        }
        finally
        {
            if (Marshal.IsComObject(instance))
            {
                Marshal.ReleaseComObject(instance);
            }
        }
    }
}

internal sealed class AudioMeterInformation(IAudioMeterInformation native) : IDisposable
{
    private IAudioMeterInformation? native = native;
    internal IReadOnlyList<float> PeakValues
    {
        get
        {
            var instance =
                native ?? throw new ObjectDisposedException(nameof(AudioMeterInformation));
            AudioOperationException.Check(instance.GetMeteringChannelCount(out var count));
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
            AudioOperationException.Check(instance.GetChannelsPeakValues(count, peaks));
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
            Marshal.ReleaseComObject(instance);
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
            Trace.TraceError(ex.ToString());
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
            Trace.TraceError(ex.ToString());
        }

        return 0;
    }

    public int OnDeviceStateChanged(string id, DeviceState state) => Notify(new(id));

    public int OnDeviceAdded(string id) => Notify(new(id));

    public int OnDeviceRemoved(string id) => Notify(new(id));

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

    public int OnPropertyValueChanged(string id, PropertyKey key) => Notify(new(id));
}
