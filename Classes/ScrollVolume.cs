using System.Windows.Forms;
using AudioSwitch.GlobalHook;

namespace AudioSwitch.Classes
{
    internal static class ScrollVolume
    {
        internal static MouseScrollHandler VolumeScroll;
        internal static bool IsEnabled;
        internal static bool ShortcutActivated;
        private static VolumeScrollKey KeysPressed;

        private static bool MouseWheel(int delta)
        {
            if (KeysPressed == Program.settings.VolumeScroll.Key && VolumeScroll != null)
            {
                ShortcutActivated = true;
                var resp = VolumeScroll(delta);
                ShortcutActivated = false;
                return resp;
            }
            return true;
        }

        private static void KeyDown(object sender, KeyEventArgs keyEventArgs)
        {
            if (keyEventArgs.Alt)
                KeysPressed |= VolumeScrollKey.Alt;
            if (keyEventArgs.Control)
                KeysPressed |= VolumeScrollKey.Control;
            if (keyEventArgs.Shift)
                KeysPressed |= VolumeScrollKey.Shift;
            if (keyEventArgs.KeyCode == Keys.LWin)
                KeysPressed |= VolumeScrollKey.LWin;
            if (keyEventArgs.KeyCode == Keys.RWin)
                KeysPressed |= VolumeScrollKey.RWin;
        }

        private static void KeyUp(object sender, KeyEventArgs keyEventArgs)
        {
            if (keyEventArgs.Alt)
                KeysPressed &= ~VolumeScrollKey.Alt;
            if (keyEventArgs.Control)
                KeysPressed &= ~VolumeScrollKey.Control;
            if (keyEventArgs.Shift)
                KeysPressed &= ~VolumeScrollKey.Shift;
            if (keyEventArgs.KeyCode == Keys.LWin)
                KeysPressed &= ~VolumeScrollKey.LWin;
            if (keyEventArgs.KeyCode == Keys.RWin)
                KeysPressed &= ~VolumeScrollKey.RWin;
        }

        private static void MouseDown(object sender, MouseEventArgs mouseEventArgs)
        {
            if (mouseEventArgs.Button == MouseButtons.Left)
                KeysPressed |= VolumeScrollKey.LeftMouseButton;
            if (mouseEventArgs.Button == MouseButtons.Right)
                KeysPressed |= VolumeScrollKey.RightMouseButton;
        }

        private static void MouseUp(object sender, MouseEventArgs mouseEventArgs)
        {
            if (mouseEventArgs.Button == MouseButtons.Left)
                KeysPressed &= ~VolumeScrollKey.LeftMouseButton;
            if (mouseEventArgs.Button == MouseButtons.Right)
                KeysPressed &= ~VolumeScrollKey.RightMouseButton;
        }

        internal static void RegisterVolScroll(bool Enable)
        {
            if (IsEnabled == Enable)
                return;
            
            if (Enable)
            {
                HookManager.MouseDown += MouseDown;
                HookManager.MouseUp += MouseUp;
                HookManager.KeyDown += KeyDown;
                HookManager.KeyUp += KeyUp;
                HookManager.MouseWheel += MouseWheel;
                IsEnabled = true;
            }
            else
            {
                HookManager.MouseDown -= MouseDown;
                HookManager.MouseUp -= MouseUp;
                HookManager.KeyDown -= KeyDown;
                HookManager.KeyUp -= KeyUp;
                HookManager.MouseWheel -= MouseWheel;
                IsEnabled = false;
            }
        }
    }
}
