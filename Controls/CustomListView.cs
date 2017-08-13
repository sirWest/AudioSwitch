using System;
using System.Drawing;
using System.Windows.Forms;
using AudioSwitch.Classes;

namespace AudioSwitch.Controls
{
    internal sealed class CustomListView : ListView
    {
        internal event ScrollEventHandler Scroll;
        Point dragOffset;

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            var parent = FindForm();
            if (parent.Name != "FormSwitcher" || !Program.settings.AlwaysVisible || e.Button != MouseButtons.Left)
                return;

            dragOffset = PointToScreen(e.Location);
            var formLocation = parent.Location;
            dragOffset.X -= formLocation.X;
            dragOffset.Y -= formLocation.Y;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            var parent = FindForm();
            if (parent.Name != "FormSwitcher" || !Program.settings.AlwaysVisible || e.Button != MouseButtons.Left)
                return;

            var newLocation = PointToScreen(e.Location);

            newLocation.X -= dragOffset.X;
            newLocation.Y -= dragOffset.Y;

            parent.Location = newLocation;
        }
        public CustomListView()
        {
            DoubleBuffered = true;
            Sorting = SortOrder.Ascending;
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
