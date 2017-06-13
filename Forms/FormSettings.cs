using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using AudioSwitch.Classes;
using AudioSwitch.CoreAudioApi;

namespace AudioSwitch.Forms
{
    public partial class FormSettings : Form
    {
        private bool HotkeysChanged;

        public FormSettings()
        {
            InitializeComponent();

            FormSwitcher.SetWindowTheme(listDevices.Handle, "explorer", null);
            var tile = new Size(listDevices.ClientSize.Width - 18, (int)(listDevices.TileSize.Height * FormSwitcher.DpiFactor));
            listDevices.TileSize = tile;

            var size = new Size((int)(32 * FormSwitcher.DpiFactor), (int)(32 * FormSwitcher.DpiFactor));
            listDevices.LargeImageList = new ImageList
            {
                ImageSize = size,
                ColorDepth = ColorDepth.Depth32Bit
            };

            Function.DataSource = Enum.GetNames(typeof(HotkeyFunction));
            Function.DataPropertyName = "HKFunction";

            HotKey.DataSource = Enum.GetNames(typeof(Keys));
            HotKey.DataPropertyName = "Key";
            
            Control.DataPropertyName = "Control";
            Alt.DataPropertyName = "Alt";
            Shift.DataPropertyName = "Shift";
            LWin.DataPropertyName = "LWin";
            RWin.DataPropertyName = "RWin";
            ShowOSD.DataPropertyName = "ShowOSD";

            gridHotkeys.DataSource = Program.settings.Hotkey;
        }

        private void FormSettings_Load(object sender, EventArgs e)
        {
            var devices = EndPoints.GetAllDeviceList();
            var cnt = 0;
            foreach (var dev in devices)
            {
                var devID = dev.Key.ID;
                var lvitem = new ListViewItem { Text = dev.Key.FriendlyName, ImageIndex = cnt, Tag = devID };

                var devSettings = Program.settings.Device.Find(x => x.DeviceID == devID);
                if (devSettings != null)
                {
                    lvitem.Font = new Font(lvitem.Font, FontStyle.Bold);

                    if (devSettings.HideFromList)
                        lvitem.Font = new Font(lvitem.Font, FontStyle.Italic);
                }

                listDevices.LargeImageList.Images.Add(dev.Value);
                listDevices.Items.Add(lvitem);
                cnt++;
            }
            pictureModded.Image = new Bitmap(Properties.Resources._66_100_highDPI);

            var OSDskins = Directory.GetDirectories(Program.Root + "Skins");
            foreach (var skinDir in OSDskins)
                comboOSDSkin.Items.Add(skinDir.Substring(skinDir.LastIndexOf('\\') + 1));
            comboOSDSkin.Text = Program.settings.OSD.Skin;
            trackTransparency.Value = Program.settings.OSD.Transparency;

            comboDefMode.Items.Add("Playback");
            comboDefMode.Items.Add("Recording");
            comboDefMode.Text = Program.settings.DefaultDataFlow == EDataFlow.eCapture ? "Recording" : "Playback";
            checkDefaultMultiAndComm.Checked = Program.settings.DefaultMultimediaAndComm;
            checkColorVU.Checked = Program.settings.ColorVU;
            checkVolScroll.Checked = Program.settings.VolumeScroll.Enabled;
            comboScrollKey.Text = Program.settings.VolumeScroll.Key.ToString();
            checkScrShowOSD.Checked = Program.settings.VolumeScroll.ShowOSD;
            checkCloseSelect.Checked = Program.settings.CloseAfterSelecting;

            if (Environment.OSVersion.Version.Major < 10)
            {
                checkCustomOSD.Checked = true;
                checkCustomOSD.Enabled = false;
            }
            else
            {
                checkCustomOSD.Checked = Program.settings.UseCustomOSD;
            }

            checkShowHWName.Checked = Program.settings.ShowHardwareName;

            radioQuickSwitch.Checked = Program.settings.QuickSwitchEnabled;
            radioAlwaysMenu.Checked = !radioQuickSwitch.Checked;
            checkQSShowOSD.Enabled = radioQuickSwitch.Checked;

            checkQSShowOSD.Checked = Program.settings.QuickSwitchShowOSD;
            
            gridHotkeys.CellEndEdit += gridHotkeys_CellEndEdit;
            gridHotkeys.RowsAdded += gridHotkeys_RowsAdded;
            gridHotkeys.RowsRemoved += gridHotkeys_RowsRemoved;
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void gridHotkeys_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            SetEditedHotKeys();
        }

        private void gridHotkeys_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
        {
            SetEditedHotKeys();
        }

        private void gridHotkeys_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            SetEditedHotKeys();
        }

        private void SetEditedHotKeys()
        {
            HotkeysChanged = true;
            buttonClose.Text = "Apply Hotkeys && Close";
        }

        private void FormSettings_FormClosing(object sender, FormClosingEventArgs e)
        {
            tabOSD_Leave(sender, e);

            if (HotkeysChanged)
            {
                var hkBad = false;
                for (var i = 0; i < gridHotkeys.Rows.Count - 1; i++)
                {
                    var cellStyle = gridHotkeys.Rows[i].DefaultCellStyle;

                    if (Program.settings.Hotkey[i].Register())
                    {
                        cellStyle.BackColor = SystemColors.Window;
                        cellStyle.SelectionBackColor = SystemColors.Highlight;
                    }
                    else
                    {
                        cellStyle.BackColor = Color.Red;
                        cellStyle.SelectionBackColor = Color.Crimson;
                        hkBad = true;
                    }
                }
                if (hkBad)
                {
                    e.Cancel = true;
                    return;
                }
            }
            Program.settings.DefaultDataFlow = comboDefMode.SelectedItem.ToString() == "Recording"
                                                   ? EDataFlow.eCapture
                                                   : EDataFlow.eRender;

            Program.settings.DefaultMultimediaAndComm = checkDefaultMultiAndComm.Checked;
            Program.settings.ColorVU = checkColorVU.Checked;
            Program.settings.ShowHardwareName = checkShowHWName.Checked;
            Program.settings.QuickSwitchEnabled = radioQuickSwitch.Checked;
            Program.settings.QuickSwitchShowOSD = checkQSShowOSD.Checked;
            Program.settings.UseCustomOSD = checkCustomOSD.Checked;
            Program.settings.CloseAfterSelecting = checkCloseSelect.Checked;

            Program.settings.Save();
        }

        private void tabOSD_Enter(object sender, EventArgs e)
        {
            labelTips.Text = "OSD window is shown. Move it and the position gets saved.";
            numTimeout.Value = Program.settings.OSD.ClosingTimeout;

            Program.frmOSD.Show();
            Program.frmOSD.LoadSkin();
            Program.frmOSD.SetVolImage(0.75f);
            Focus();
        }

        private void tabOSD_Leave(object sender, EventArgs e)
        {
            Program.frmOSD.Hide();
        }

        private void numTimeout_ValueChanged(object sender, EventArgs e)
        {
            Program.settings.OSD.ClosingTimeout = (int)numTimeout.Value;
        }

        private void checkDefaultMultiAndComm_CheckedChanged(object sender, EventArgs e)
        {
            Program.settings.DefaultMultimediaAndComm = checkDefaultMultiAndComm.Checked;
        }

        private void trackTransparency_ValueChanged(object sender, EventArgs e)
        {
            Program.settings.OSD.Transparency = Program.frmOSD.Transparency = (byte)trackTransparency.Value;
            Program.frmOSD.SetVolImage(0.75f);
        }

        private void trackBarsHSB_Scroll(object sender, EventArgs e)
        {
            pictureModded.Image?.Dispose();
            pictureModded.Image = DeviceIcons.ChangeColors(new Bitmap(Properties.Resources._66_100_highDPI), trackHue.Value, trackSaturation.Value / 100f, trackBrightness.Value/100f);
        }

        private void buttonResetDevice_Click(object sender, EventArgs e)
        {
            trackHue.Value = 0;
            trackSaturation.Value = 0;
            trackBrightness.Value = 0;
            pictureModded.Image = new Bitmap(Properties.Resources._66_100_highDPI);
            checkHideDevice.Checked = false;
            checkCustomName.Checked = false;
            textCustomName.Enabled = false;
            textCustomName.Clear();

            listDevices.SelectedItems[0].Font = new Font(listDevices.SelectedItems[0].Font, FontStyle.Regular);

            var devSettings = Program.settings.Device.Find(x => x.DeviceID == (string)listDevices.SelectedItems[0].Tag);
            if (devSettings != null)
                Program.settings.Device.Remove(devSettings);
        }

        private void buttonSaveDevice_Click(object sender, EventArgs e)
        {
            if (listDevices.SelectedItems.Count == 0) return;

            var devSettings = Program.settings.Device.Find(x => x.DeviceID == (string) listDevices.SelectedItems[0].Tag);

            listDevices.SelectedItems[0].Font = new Font(listDevices.SelectedItems[0].Font,
                                                         checkHideDevice.Checked ? FontStyle.Italic : FontStyle.Bold);

            if (devSettings != null)
            {
                devSettings.Brightness = trackBrightness.Value;
                devSettings.Hue = trackHue.Value;
                devSettings.Saturation = trackSaturation.Value;
                devSettings.HideFromList = checkHideDevice.Checked;
                devSettings.UseCustomName = checkCustomName.Checked;
                devSettings.CustomName = textCustomName.Text;
            }
            else
            {
                devSettings = new Settings.CDevice
                    {
                        DeviceID = (string) listDevices.SelectedItems[0].Tag,
                        HideFromList = checkHideDevice.Checked,
                        Brightness = trackBrightness.Value,
                        Hue = trackHue.Value,
                        Saturation = trackSaturation.Value,
                        UseCustomName = checkCustomName.Checked,
                        CustomName = textCustomName.Text
                    };
                Program.settings.Device.Add(devSettings);
            }
        }

        private void listDevices_SelectedIndexChanged(object sender, EventArgs e)
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
        }

        private void comboOSDSkin_SelectedIndexChanged(object sender, EventArgs e)
        {
            Program.settings.OSD.Skin = comboOSDSkin.Text;
            Program.frmOSD.LoadSkin();
            Program.frmOSD.SetVolImage(0.75f);

            labelAuthor.Text = Program.frmOSD.Skin.Author;
            linkWebpage.Text = Program.frmOSD.Skin.Website;
            labelVersion.Text = Program.frmOSD.Skin.Version;
            Focus();
        }

        private void linkWebpage_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (linkWebpage.Links.Count > 0)
                Process.Start(linkWebpage.Text);
        }

        private void checkVolScroll_CheckedChanged(object sender, EventArgs e)
        {
            comboScrollKey.Enabled = checkVolScroll.Checked;
            checkScrShowOSD.Enabled = checkVolScroll.Checked;
            labelVolScroll.Enabled = checkVolScroll.Checked;
            Program.settings.VolumeScroll.Enabled = checkVolScroll.Checked;
        }

        private void comboScrollKey_SelectedIndexChanged(object sender, EventArgs e)
        {
            Program.settings.VolumeScroll.Key =
                (VolumeScrollKey) Enum.Parse(typeof (VolumeScrollKey), comboScrollKey.Text);
        }

        private void checkScrShowOSD_CheckedChanged(object sender, EventArgs e)
        {
            Program.settings.VolumeScroll.ShowOSD = checkScrShowOSD.Checked;
        }

        private void tabDevices_Enter(object sender, EventArgs e)
        {
            labelTips.Text = "Select a device and change it's icon appearance. Don't forget to save!";
        }

        private void tabHotkeys_Enter(object sender, EventArgs e)
        {
            labelTips.Text = "Select a hotkey row and press 'Delete' keyboard button to delete it.";
        }

        private void radioQuickSwitch_CheckedChanged(object sender, EventArgs e)
        {
            checkQSShowOSD.Enabled = radioQuickSwitch.Checked;
        }

        private void checkCustomOSD_CheckedChanged(object sender, EventArgs e)
        {
            groupOSD.Enabled = checkCustomOSD.Checked;
        }

        private void checkCustomName_CheckedChanged(object sender, EventArgs e)
        {
            textCustomName.Enabled = checkCustomName.Checked;
        }
    }
}
