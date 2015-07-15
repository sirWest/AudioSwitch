using System;
using System.Windows.Forms;

namespace AudioSwitch.Controls
{
    internal sealed class CustomListView : ListView
    {
        internal event ScrollEventHandler Scroll;

        public CustomListView()
        {
            DoubleBuffered = true;
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 522 && Scroll != null)
            {
                var bytes = BitConverter.GetBytes((int) m.WParam);
                var y = BitConverter.ToInt16(bytes, 2);
                Scroll(this, new ScrollEventArgs((ScrollEventType) (m.WParam.ToInt32() & 0xffff), y));
            }
            else
                base.WndProc(ref m);
        }
    }
}
