using AudioSwitch.Classes;
using AudioSwitch.CoreAudioApi;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace AudioSwitch.Forms
{
    public partial class Devices : UserControl
    {
        public Devices()
        {
            InitializeComponent();
        }

        public void PostConstructor(EDataFlow dataFlow)
        {
            var devices = EDataFlow.eRender.Equals(dataFlow) ? DeviceRepository.FindPlayBackDevices() : DeviceRepository.FindCaptureDevices();
            var cnt = 0;
            foreach (var dev in devices)
            {
                var devID = dev.MMDevice.ID;
                var lvitem = new ListViewItem { Text = dev.MMDevice.FriendlyName, ImageIndex = cnt, Tag = devID };

                var devSettings = Program.settings.Device.Find(x => x.DeviceID == devID);
                if (devSettings != null)
                {
                    lvitem.Font = new Font(lvitem.Font, FontStyle.Bold);

                    if (devSettings.HideFromList)
                        lvitem.Font = new Font(lvitem.Font, FontStyle.Italic);
                }

                listDevices.LargeImageList.Images.Add(DeviceIcons.GetIcon(dev.MMDevice.IconPath));
                listDevices.Items.Add(lvitem);
                cnt++;
            }
        }


        private void Devices_Load(object sender, EventArgs e)
        {
            pictureModded.Image = new Bitmap(Properties.Resources._66_100_highDPI);

            FormSwitcher.SetWindowTheme(listDevices.Handle, "explorer", null);

            var tile = new Size(listDevices.ClientSize.Width - 18, (int)(listDevices.TileSize.Height * FormSwitcher.DpiFactor));
            if(tile.Width < 1 || tile.Height < 1)
            {
                tile = new Size(15, 15);
            }
            listDevices.TileSize = tile;
            var size = new Size((int)(32 * FormSwitcher.DpiFactor), (int)(32 * FormSwitcher.DpiFactor));
            if (size.Width < 1 || size.Height < 1)
            {
                size = new Size(15, 15);
            }
            listDevices.LargeImageList = new ImageList
            {
                ImageSize = size,
                ColorDepth = ColorDepth.Depth32Bit
            };


        }

        private void buttonRemove_Click(object sender, EventArgs e)
        {
            trackHue.Value = 0;
            trackSaturation.Value = 0;
            trackBrightness.Value = 0;
            pictureModded.Image = new Bitmap(Properties.Resources._66_100_highDPI);
            checkHideDevice.Checked = false;
            checkCustomName.Checked = false;
            textCustomName.Enabled = false;
            textCustomName.Clear();
            checkEnableAsMultimediaOnStartup.Checked = false;
            checkEnableAsCommunicationsOnStartup.Checked = false;

            listDevices.SelectedItems[0].Font = new Font(listDevices.SelectedItems[0].Font, FontStyle.Regular);

            var devSettings = Program.settings.Device.Find(x => x.DeviceID == (string)listDevices.SelectedItems[0].Tag);
            if (devSettings != null)
                Program.settings.Device.Remove(devSettings);
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            if (listDevices.SelectedItems.Count == 0) return;

            var devSettings = Program.settings.Device.Find(x => x.DeviceID == (string)listDevices.SelectedItems[0].Tag);

            listDevices.SelectedItems[0].Font = new Font(listDevices.SelectedItems[0].Font,
                                                         checkHideDevice.Checked ? FontStyle.Italic : FontStyle.Bold);
            IEnumerable<ListViewItem> lv = listDevices.Items.Cast<ListViewItem>();

            if (checkEnableAsMultimediaOnStartup.Checked)
            {
                foreach(var item in lv) { 
                    var settings = Program.settings.Device.Find(x => x.DeviceID == (string)item.Tag);
                    if (settings != null)
                    {
                        settings.DefaultMultimediaDevice = false;
                    }
                }
            }
            if (checkEnableAsCommunicationsOnStartup.Checked)
            {
                foreach (var item in lv)
                {
                    var settings = Program.settings.Device.Find(x => x.DeviceID == (string)item.Tag);
                    if (settings != null)
                    {
                        settings.DefaultCommunicationsDevice = false;
                    }
                }
            }

            if (devSettings != null)
            {
                devSettings.Brightness = trackBrightness.Value;
                devSettings.Hue = trackHue.Value;
                devSettings.Saturation = trackSaturation.Value;
                devSettings.HideFromList = checkHideDevice.Checked;
                devSettings.UseCustomName = checkCustomName.Checked;
                devSettings.CustomName = textCustomName.Text;
                devSettings.DefaultMultimediaDevice = checkEnableAsMultimediaOnStartup.Checked;
                devSettings.DefaultCommunicationsDevice = checkEnableAsCommunicationsOnStartup.Checked;
            }
            else
            {
                devSettings = new Settings.CDevice
                {
                    DeviceID = (string)listDevices.SelectedItems[0].Tag,
                    HideFromList = checkHideDevice.Checked,
                    Brightness = trackBrightness.Value,
                    Hue = trackHue.Value,
                    Saturation = trackSaturation.Value,
                    UseCustomName = checkCustomName.Checked,
                    CustomName = textCustomName.Text,
                    DefaultMultimediaDevice = checkEnableAsMultimediaOnStartup.Checked,
                    DefaultCommunicationsDevice = checkEnableAsCommunicationsOnStartup.Checked

                };
                Program.settings.Device.Add(devSettings);
            }

        }


        private void listDevices_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            if (listDevices.SelectedItems.Count == 0) return;

            var devSettings = Program.settings.Device.Find(x => x.DeviceID == (string)listDevices.SelectedItems[0].Tag);
            if (devSettings == null)
            {
                trackBrightness.Value = 0;
                trackHue.Value = 0;
                trackSaturation.Value = 0;
                pictureModded.Image = new Bitmap(Properties.Resources._66_100_highDPI);
                checkHideDevice.Checked = false;
                checkCustomName.Checked = false;
                textCustomName.Enabled = false;
                textCustomName.Clear();
                checkEnableAsMultimediaOnStartup.Checked = false;
                checkEnableAsCommunicationsOnStartup.Checked = false;

                return;
            }

            trackBrightness.Value = devSettings.Brightness;
            trackHue.Value = devSettings.Hue;
            trackSaturation.Value = devSettings.Saturation;

            pictureModded.Image?.Dispose();
            pictureModded.Image = DeviceIcons.ChangeColors(new Bitmap(Properties.Resources._66_100_highDPI), trackHue.Value, trackSaturation.Value / 100f, trackBrightness.Value / 100f);
            checkHideDevice.Checked = devSettings.HideFromList;

            checkCustomName.Checked = devSettings.UseCustomName;
            textCustomName.Text = devSettings.CustomName;
            checkEnableAsMultimediaOnStartup.Checked = devSettings.DefaultMultimediaDevice;
            checkEnableAsCommunicationsOnStartup.Checked = devSettings.DefaultCommunicationsDevice;
        }

        private void checkCustomName_CheckedChanged_1(object sender, EventArgs e)
        {
            textCustomName.Enabled = checkCustomName.Checked;

        }

        private void trackBarsHSB_Scroll()
        {
            pictureModded.Image?.Dispose();
            pictureModded.Image = DeviceIcons.ChangeColors(new Bitmap(Properties.Resources._66_100_highDPI), trackHue.Value, trackSaturation.Value / 100f, trackBrightness.Value / 100f);
        }

        private void trackHue_Scroll(object sender, EventArgs e)
        {
            trackBarsHSB_Scroll();
        }

        private void trackSaturation_Scroll(object sender, EventArgs e)
        {
            trackBarsHSB_Scroll();
        }

        private void trackBrightness_Scroll(object sender, EventArgs e)
        {
            trackBarsHSB_Scroll();
        }
    }
}
