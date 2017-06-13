using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using AudioSwitch.Classes;
using AudioSwitch.CoreAudioApi;
using System.Collections.Generic;

namespace AudioSwitch.Forms
{
    public partial class FormSettings : Form
    {
        private bool HotkeysChanged;

        public FormSettings()
        {
            InitializeComponent();




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
        }

        private void buttonResetDevice_Click(object sender, EventArgs e)
        {

        }

        private void buttonSaveDevice_Click(object sender, EventArgs e)
        {

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



        private void devices2_Load(object sender, EventArgs e)
        {
            playbackDevices.PostConstructor(EDataFlow.eRender);
        }

        private void recordingDevices_Load(object sender, EventArgs e)
        {
            recordingDevices.PostConstructor(EDataFlow.eCapture);
        }
    }
}
