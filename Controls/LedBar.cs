using System;
using System.Drawing;
using System.Windows.Forms;

namespace AudioSwitch.Controls
{
    internal sealed partial class LedBar : UserControl
    {
        internal event ScrollEventHandler Scroll;
        private readonly Label[] LED;
        private int lastValue;

        private bool _oldStyle;
        public bool OldStyle
        {
            get
            {
                return _oldStyle;
            }
            set
            {
                BackColor = value ? Color.Black : SystemColors.Control;
                _oldStyle = value;
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 522)
            {
                if (Scroll != null)
                {
                    var bytes = BitConverter.GetBytes((int)m.WParam);
                    var y = BitConverter.ToInt16(bytes, 2);
                    Scroll(this, new ScrollEventArgs((ScrollEventType)(m.WParam.ToInt32() & 0xffff), y));
                }
            }
            else
                base.WndProc(ref m);
        }

        private static readonly Color[] pgOnColors =
        {
            Color.Black, Color.Lime, Color.Lime, Color.Lime, Color.Lime,
            Color.Lime, Color.Lime, Color.Lime, Color.Lime, Color.Lime,
            Color.Yellow, Color.Yellow, Color.Yellow, Color.Red
        };

        private static readonly Color[] pgOffColors =
        {
            Color.Black, Color.Green, Color.Green, Color.Green, Color.Green,
            Color.Green, Color.Green, Color.Green, Color.Green, Color.Green,
            Color.Olive, Color.Olive, Color.Olive, Color.Maroon
        };

        public LedBar()
        {
            InitializeComponent();
            LED = new[] { new Label(), led1, led2, led3, led4, led5, led6, led7, led8, led9, led10, led11, led12, led13 };
        }

        internal void SetValue(float value)
        {
            var val = (int)Math.Ceiling(value * 14);
            if (lastValue == val) return;
            lastValue = val;

            if (OldStyle)
                for (var i = 0; i < 14; i++)
                    LED[i].BackColor = val >= i ? pgOnColors[i] : pgOffColors[i];
            else
                for (var i = 0; i < 14; i++)
                    LED[i].BackColor = val >= i ? Color.Gray : Color.Gainsboro;
        }
    }
}
