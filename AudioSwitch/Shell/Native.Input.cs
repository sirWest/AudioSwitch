using System.Diagnostics;
using System.Runtime.InteropServices;

namespace AudioSwitch.Shell;

internal static partial class Native
{
    private static readonly Input[] ModifierMask =
    [
        new()
        {
            Type = 1,
            Data = new() { Keyboard = new() { VirtualKey = 0xe8 } },
        },
        new()
        {
            Type = 1,
            Data = new()
            {
                Keyboard = new() { VirtualKey = 0xe8, Flags = 2 },
            },
        },
    ];

    internal static void MaskModifierTap()
    {
        // VK_E8 is unassigned: mark the modifier as part of a chord without typing
        // text or invoking another shortcut. Send down/up together, inside the hook.
        if (
            SendInput((uint)ModifierMask.Length, ModifierMask, Marshal.SizeOf<Input>())
            != ModifierMask.Length
        )
        {
            Trace.TraceWarning($"Could not mask scroll modifier: {Marshal.GetLastWin32Error()}");
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct Input
    {
        public uint Type;
        public InputData Data;
    }

    [StructLayout(LayoutKind.Explicit)]
    internal struct InputData
    {
        [FieldOffset(0)]
        public KeyboardInput Keyboard;

        // INPUT's union must include the larger MOUSEINPUT for correct x86/x64 sizing.
        [FieldOffset(0)]
        public MouseInput Mouse;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct KeyboardInput
    {
        public ushort VirtualKey;
        public ushort ScanCode;
        public uint Flags;
        public uint Time;
        public nuint ExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct MouseInput
    {
        public int X;
        public int Y;
        public uint MouseData;
        public uint Flags;
        public uint Time;
        public nuint ExtraInfo;
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(uint count, [In] Input[] inputs, int size);
}
