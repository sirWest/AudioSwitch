using System;
using AudioSwitch.Classes;
using AudioSwitch.Controls;

namespace AudioSwitch.Forms
{
    partial class FormSettings
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabGeneral = new System.Windows.Forms.TabPage();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.checkVolScroll = new System.Windows.Forms.CheckBox();
            this.comboScrollKey = new System.Windows.Forms.ComboBox();
            this.labelVolScroll = new System.Windows.Forms.Label();
            this.checkScrShowOSD = new System.Windows.Forms.CheckBox();
            this.checkCustomOSD = new System.Windows.Forms.CheckBox();
            this.groupOSD = new System.Windows.Forms.GroupBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.numTimeout = new System.Windows.Forms.NumericUpDown();
            this.label8 = new System.Windows.Forms.Label();
            this.trackTransparency = new System.Windows.Forms.TrackBar();
            this.labelAuthor = new System.Windows.Forms.Label();
            this.linkWebpage = new System.Windows.Forms.LinkLabel();
            this.labelVersion = new System.Windows.Forms.Label();
            this.comboOSDSkin = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.checkCloseSelect = new System.Windows.Forms.CheckBox();
            this.comboDefMode = new System.Windows.Forms.ComboBox();
            this.checkQSShowOSD = new System.Windows.Forms.CheckBox();
            this.radioQuickSwitch = new System.Windows.Forms.RadioButton();
            this.radioAlwaysMenu = new System.Windows.Forms.RadioButton();
            this.label5 = new System.Windows.Forms.Label();
            this.checkShowHWName = new System.Windows.Forms.CheckBox();
            this.checkColorVU = new System.Windows.Forms.CheckBox();
            this.label6 = new System.Windows.Forms.Label();
            this.checkDefaultMultiAndComm = new System.Windows.Forms.CheckBox();
            this.tabPlaybackDevices = new System.Windows.Forms.TabPage();
            this.tabRecordingDevices = new System.Windows.Forms.TabPage();
            this.tabHotkeys = new System.Windows.Forms.TabPage();
            this.gridHotkeys = new System.Windows.Forms.DataGridView();
            this.Function = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.Control = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.Alt = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.Shift = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.LWin = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.RWin = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.ShowOSD = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.HotKey = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.buttonClose = new System.Windows.Forms.Button();
            this.labelTips = new System.Windows.Forms.Label();
            this.playbackDevices = new AudioSwitch.Controls.Devices();
            this.recordingDevices = new AudioSwitch.Controls.Devices();
            this.tabControl.SuspendLayout();
            this.tabGeneral.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupOSD.SuspendLayout();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numTimeout)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackTransparency)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.tabPlaybackDevices.SuspendLayout();
            this.tabRecordingDevices.SuspendLayout();
            this.tabHotkeys.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridHotkeys)).BeginInit();
            this.SuspendLayout();
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.tabGeneral);
            this.tabControl.Controls.Add(this.tabPlaybackDevices);
            this.tabControl.Controls.Add(this.tabRecordingDevices);
            this.tabControl.Controls.Add(this.tabHotkeys);
            this.tabControl.HotTrack = true;
            this.tabControl.Location = new System.Drawing.Point(2, 3);
            this.tabControl.Margin = new System.Windows.Forms.Padding(2);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(536, 352);
            this.tabControl.TabIndex = 0;
            // 
            // tabGeneral
            // 
            this.tabGeneral.Controls.Add(this.groupBox4);
            this.tabGeneral.Controls.Add(this.checkCustomOSD);
            this.tabGeneral.Controls.Add(this.groupOSD);
            this.tabGeneral.Controls.Add(this.groupBox2);
            this.tabGeneral.Location = new System.Drawing.Point(4, 22);
            this.tabGeneral.Margin = new System.Windows.Forms.Padding(2);
            this.tabGeneral.Name = "tabGeneral";
            this.tabGeneral.Size = new System.Drawing.Size(528, 326);
            this.tabGeneral.TabIndex = 2;
            this.tabGeneral.Text = "General";
            this.tabGeneral.UseVisualStyleBackColor = true;
            this.tabGeneral.Enter += new System.EventHandler(this.tabOSD_Enter);
            this.tabGeneral.Leave += new System.EventHandler(this.tabOSD_Leave);
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.checkVolScroll);
            this.groupBox4.Controls.Add(this.comboScrollKey);
            this.groupBox4.Controls.Add(this.labelVolScroll);
            this.groupBox4.Controls.Add(this.checkScrShowOSD);
            this.groupBox4.Location = new System.Drawing.Point(355, 6);
            this.groupBox4.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox4.Size = new System.Drawing.Size(169, 115);
            this.groupBox4.TabIndex = 1;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Volume Scrolling";
            // 
            // checkVolScroll
            // 
            this.checkVolScroll.Location = new System.Drawing.Point(11, 19);
            this.checkVolScroll.Margin = new System.Windows.Forms.Padding(2);
            this.checkVolScroll.Name = "checkVolScroll";
            this.checkVolScroll.Size = new System.Drawing.Size(61, 21);
            this.checkVolScroll.TabIndex = 0;
            this.checkVolScroll.Text = "Enabled";
            this.checkVolScroll.UseVisualStyleBackColor = true;
            this.checkVolScroll.CheckedChanged += new System.EventHandler(this.checkVolScroll_CheckedChanged);
            // 
            // comboScrollKey
            // 
            this.comboScrollKey.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboScrollKey.FormattingEnabled = true;
            this.comboScrollKey.Items.AddRange(new object[] {
            "LeftMouseButton",
            "RightMouseButton",
            "Control",
            "Alt",
            "LWin",
            "RWin",
            "Shift",
            "LeftMouseButton",
            "RightMouseButton",
            "Control",
            "Alt",
            "LWin",
            "RWin",
            "Shift"});
            this.comboScrollKey.Location = new System.Drawing.Point(11, 42);
            this.comboScrollKey.Margin = new System.Windows.Forms.Padding(2);
            this.comboScrollKey.Name = "comboScrollKey";
            this.comboScrollKey.Size = new System.Drawing.Size(149, 21);
            this.comboScrollKey.TabIndex = 1;
            this.comboScrollKey.SelectedIndexChanged += new System.EventHandler(this.comboScrollKey_SelectedIndexChanged);
            // 
            // labelVolScroll
            // 
            this.labelVolScroll.Location = new System.Drawing.Point(202, 20);
            this.labelVolScroll.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelVolScroll.Name = "labelVolScroll";
            this.labelVolScroll.Size = new System.Drawing.Size(100, 19);
            this.labelVolScroll.TabIndex = 23;
            this.labelVolScroll.Text = " + Mouse Wheel";
            // 
            // checkScrShowOSD
            // 
            this.checkScrShowOSD.Location = new System.Drawing.Point(11, 69);
            this.checkScrShowOSD.Margin = new System.Windows.Forms.Padding(2);
            this.checkScrShowOSD.Name = "checkScrShowOSD";
            this.checkScrShowOSD.Size = new System.Drawing.Size(90, 20);
            this.checkScrShowOSD.TabIndex = 2;
            this.checkScrShowOSD.Text = "Show OSD";
            this.checkScrShowOSD.UseVisualStyleBackColor = true;
            this.checkScrShowOSD.CheckedChanged += new System.EventHandler(this.checkScrShowOSD_CheckedChanged);
            // 
            // checkCustomOSD
            // 
            this.checkCustomOSD.AutoSize = true;
            this.checkCustomOSD.BackColor = System.Drawing.Color.Transparent;
            this.checkCustomOSD.Checked = true;
            this.checkCustomOSD.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkCustomOSD.Location = new System.Drawing.Point(224, 6);
            this.checkCustomOSD.Margin = new System.Windows.Forms.Padding(2);
            this.checkCustomOSD.Name = "checkCustomOSD";
            this.checkCustomOSD.Size = new System.Drawing.Size(127, 17);
            this.checkCustomOSD.TabIndex = 19;
            this.checkCustomOSD.Text = "Use customized OSD";
            this.checkCustomOSD.UseVisualStyleBackColor = false;
            this.checkCustomOSD.CheckedChanged += new System.EventHandler(this.checkCustomOSD_CheckedChanged);
            // 
            // groupOSD
            // 
            this.groupOSD.Controls.Add(this.groupBox3);
            this.groupOSD.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(186)));
            this.groupOSD.Location = new System.Drawing.Point(7, 7);
            this.groupOSD.Margin = new System.Windows.Forms.Padding(2);
            this.groupOSD.Name = "groupOSD";
            this.groupOSD.Padding = new System.Windows.Forms.Padding(2);
            this.groupOSD.Size = new System.Drawing.Size(344, 114);
            this.groupOSD.TabIndex = 18;
            this.groupOSD.TabStop = false;
            this.groupOSD.Text = "OSD Settings";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.label3);
            this.groupBox3.Controls.Add(this.numTimeout);
            this.groupBox3.Controls.Add(this.label8);
            this.groupBox3.Controls.Add(this.trackTransparency);
            this.groupBox3.Controls.Add(this.labelAuthor);
            this.groupBox3.Controls.Add(this.linkWebpage);
            this.groupBox3.Controls.Add(this.labelVersion);
            this.groupBox3.Controls.Add(this.comboOSDSkin);
            this.groupBox3.Controls.Add(this.label1);
            this.groupBox3.Location = new System.Drawing.Point(6, 13);
            this.groupBox3.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox3.Size = new System.Drawing.Size(332, 97);
            this.groupBox3.TabIndex = 0;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Skin";
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(181, 72);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(86, 18);
            this.label3.TabIndex = 20;
            this.label3.Text = "Closing Timeout (ms):";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // numTimeout
            // 
            this.numTimeout.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.numTimeout.Location = new System.Drawing.Point(271, 70);
            this.numTimeout.Margin = new System.Windows.Forms.Padding(2);
            this.numTimeout.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.numTimeout.Minimum = new decimal(new int[] {
            500,
            0,
            0,
            -2147483648});
            this.numTimeout.Name = "numTimeout";
            this.numTimeout.Size = new System.Drawing.Size(53, 20);
            this.numTimeout.TabIndex = 19;
            this.numTimeout.Value = new decimal(new int[] {
            2000,
            0,
            0,
            0});
            this.numTimeout.ValueChanged += new System.EventHandler(this.numTimeout_ValueChanged);
            // 
            // label8
            // 
            this.label8.Location = new System.Drawing.Point(218, 44);
            this.label8.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(80, 18);
            this.label8.TabIndex = 18;
            this.label8.Text = "Transparency";
            // 
            // trackTransparency
            // 
            this.trackTransparency.BackColor = System.Drawing.SystemColors.Window;
            this.trackTransparency.Location = new System.Drawing.Point(196, 18);
            this.trackTransparency.Margin = new System.Windows.Forms.Padding(2);
            this.trackTransparency.Maximum = 255;
            this.trackTransparency.Name = "trackTransparency";
            this.trackTransparency.Size = new System.Drawing.Size(128, 45);
            this.trackTransparency.TabIndex = 17;
            this.trackTransparency.TickStyle = System.Windows.Forms.TickStyle.None;
            this.trackTransparency.Value = 255;
            this.trackTransparency.ValueChanged += new System.EventHandler(this.trackTransparency_ValueChanged);
            // 
            // labelAuthor
            // 
            this.labelAuthor.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(186)));
            this.labelAuthor.Location = new System.Drawing.Point(11, 41);
            this.labelAuthor.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelAuthor.Name = "labelAuthor";
            this.labelAuthor.Size = new System.Drawing.Size(181, 18);
            this.labelAuthor.TabIndex = 8;
            this.labelAuthor.Text = "<Author>";
            this.labelAuthor.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // linkWebpage
            // 
            this.linkWebpage.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.linkWebpage.LinkColor = System.Drawing.SystemColors.HotTrack;
            this.linkWebpage.Location = new System.Drawing.Point(11, 70);
            this.linkWebpage.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.linkWebpage.Name = "linkWebpage";
            this.linkWebpage.Size = new System.Drawing.Size(201, 17);
            this.linkWebpage.TabIndex = 15;
            this.linkWebpage.TabStop = true;
            this.linkWebpage.Text = "<Webpage>";
            this.linkWebpage.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.linkWebpage.VisitedLinkColor = System.Drawing.SystemColors.HotTrack;
            this.linkWebpage.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkWebpage_LinkClicked);
            // 
            // labelVersion
            // 
            this.labelVersion.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(186)));
            this.labelVersion.Location = new System.Drawing.Point(51, 59);
            this.labelVersion.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelVersion.Name = "labelVersion";
            this.labelVersion.Size = new System.Drawing.Size(44, 10);
            this.labelVersion.TabIndex = 16;
            this.labelVersion.Text = "<ver>";
            // 
            // comboOSDSkin
            // 
            this.comboOSDSkin.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboOSDSkin.FormattingEnabled = true;
            this.comboOSDSkin.Location = new System.Drawing.Point(14, 18);
            this.comboOSDSkin.Margin = new System.Windows.Forms.Padding(2);
            this.comboOSDSkin.Name = "comboOSDSkin";
            this.comboOSDSkin.Size = new System.Drawing.Size(178, 21);
            this.comboOSDSkin.TabIndex = 0;
            this.comboOSDSkin.SelectedIndexChanged += new System.EventHandler(this.comboOSDSkin_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(186)));
            this.label1.Location = new System.Drawing.Point(12, 59);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(54, 11);
            this.label1.TabIndex = 8;
            this.label1.Text = "Version";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.checkCloseSelect);
            this.groupBox2.Controls.Add(this.comboDefMode);
            this.groupBox2.Controls.Add(this.checkQSShowOSD);
            this.groupBox2.Controls.Add(this.radioQuickSwitch);
            this.groupBox2.Controls.Add(this.radioAlwaysMenu);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.checkShowHWName);
            this.groupBox2.Controls.Add(this.checkColorVU);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.checkDefaultMultiAndComm);
            this.groupBox2.Location = new System.Drawing.Point(7, 125);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox2.Size = new System.Drawing.Size(517, 159);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "General Behavior";
            // 
            // checkCloseSelect
            // 
            this.checkCloseSelect.Location = new System.Drawing.Point(12, 121);
            this.checkCloseSelect.Margin = new System.Windows.Forms.Padding(2);
            this.checkCloseSelect.Name = "checkCloseSelect";
            this.checkCloseSelect.Size = new System.Drawing.Size(186, 24);
            this.checkCloseSelect.TabIndex = 26;
            this.checkCloseSelect.Text = "Close menu after selecting device";
            this.checkCloseSelect.UseVisualStyleBackColor = true;
            // 
            // comboDefMode
            // 
            this.comboDefMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboDefMode.FormattingEnabled = true;
            this.comboDefMode.Location = new System.Drawing.Point(78, 18);
            this.comboDefMode.Margin = new System.Windows.Forms.Padding(2);
            this.comboDefMode.Name = "comboDefMode";
            this.comboDefMode.Size = new System.Drawing.Size(120, 21);
            this.comboDefMode.TabIndex = 0;
            // 
            // checkQSShowOSD
            // 
            this.checkQSShowOSD.AutoSize = true;
            this.checkQSShowOSD.Location = new System.Drawing.Point(300, 100);
            this.checkQSShowOSD.Margin = new System.Windows.Forms.Padding(2);
            this.checkQSShowOSD.Name = "checkQSShowOSD";
            this.checkQSShowOSD.Size = new System.Drawing.Size(79, 17);
            this.checkQSShowOSD.TabIndex = 6;
            this.checkQSShowOSD.Text = "Show OSD";
            this.checkQSShowOSD.UseVisualStyleBackColor = true;
            // 
            // radioQuickSwitch
            // 
            this.radioQuickSwitch.Location = new System.Drawing.Point(283, 57);
            this.radioQuickSwitch.Margin = new System.Windows.Forms.Padding(2);
            this.radioQuickSwitch.Name = "radioQuickSwitch";
            this.radioQuickSwitch.Size = new System.Drawing.Size(207, 39);
            this.radioQuickSwitch.TabIndex = 5;
            this.radioQuickSwitch.Text = "menu when AudioSwitch is open, otherwise quick-switches device";
            this.radioQuickSwitch.UseVisualStyleBackColor = true;
            this.radioQuickSwitch.CheckedChanged += new System.EventHandler(this.radioQuickSwitch_CheckedChanged);
            // 
            // radioAlwaysMenu
            // 
            this.radioAlwaysMenu.Checked = true;
            this.radioAlwaysMenu.Location = new System.Drawing.Point(283, 32);
            this.radioAlwaysMenu.Margin = new System.Windows.Forms.Padding(2);
            this.radioAlwaysMenu.Name = "radioAlwaysMenu";
            this.radioAlwaysMenu.Size = new System.Drawing.Size(140, 24);
            this.radioAlwaysMenu.TabIndex = 4;
            this.radioAlwaysMenu.TabStop = true;
            this.radioAlwaysMenu.Text = "always menu";
            this.radioAlwaysMenu.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(274, 18);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(149, 13);
            this.label5.TabIndex = 25;
            this.label5.Text = "Right-clicking tray icon opens:";
            // 
            // checkShowHWName
            // 
            this.checkShowHWName.AutoSize = true;
            this.checkShowHWName.Location = new System.Drawing.Point(12, 79);
            this.checkShowHWName.Margin = new System.Windows.Forms.Padding(2);
            this.checkShowHWName.Name = "checkShowHWName";
            this.checkShowHWName.Size = new System.Drawing.Size(186, 17);
            this.checkShowHWName.TabIndex = 2;
            this.checkShowHWName.Text = "Show also device hardware name";
            this.checkShowHWName.UseVisualStyleBackColor = true;
            // 
            // checkColorVU
            // 
            this.checkColorVU.Location = new System.Drawing.Point(12, 102);
            this.checkColorVU.Margin = new System.Windows.Forms.Padding(2);
            this.checkColorVU.Name = "checkColorVU";
            this.checkColorVU.Size = new System.Drawing.Size(108, 17);
            this.checkColorVU.TabIndex = 3;
            this.checkColorVU.Text = "Color LED VU meter";
            this.checkColorVU.UseVisualStyleBackColor = true;
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(9, 21);
            this.label6.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(87, 19);
            this.label6.TabIndex = 18;
            this.label6.Text = "Default GUI Devices:";
            // 
            // checkDefaultMultiAndComm
            // 
            this.checkDefaultMultiAndComm.Location = new System.Drawing.Point(12, 56);
            this.checkDefaultMultiAndComm.Margin = new System.Windows.Forms.Padding(2);
            this.checkDefaultMultiAndComm.Name = "checkDefaultMultiAndComm";
            this.checkDefaultMultiAndComm.Size = new System.Drawing.Size(252, 19);
            this.checkDefaultMultiAndComm.TabIndex = 1;
            this.checkDefaultMultiAndComm.Text = "Switch also default communications device";
            this.checkDefaultMultiAndComm.UseVisualStyleBackColor = true;
            this.checkDefaultMultiAndComm.CheckedChanged += new System.EventHandler(this.checkDefaultMultiAndComm_CheckedChanged);
            // 
            // tabPlaybackDevices
            // 
            this.tabPlaybackDevices.Controls.Add(this.playbackDevices);
            this.tabPlaybackDevices.Location = new System.Drawing.Point(4, 22);
            this.tabPlaybackDevices.Margin = new System.Windows.Forms.Padding(2);
            this.tabPlaybackDevices.Name = "tabPlaybackDevices";
            this.tabPlaybackDevices.Padding = new System.Windows.Forms.Padding(2);
            this.tabPlaybackDevices.Size = new System.Drawing.Size(528, 326);
            this.tabPlaybackDevices.TabIndex = 1;
            this.tabPlaybackDevices.Text = "Playback Devices";
            this.tabPlaybackDevices.UseVisualStyleBackColor = true;
            this.tabPlaybackDevices.Enter += new System.EventHandler(this.tabDevices_Enter);
            // 
            // tabRecordingDevices
            // 
            this.tabRecordingDevices.Controls.Add(this.recordingDevices);
            this.tabRecordingDevices.Location = new System.Drawing.Point(4, 22);
            this.tabRecordingDevices.Name = "tabRecordingDevices";
            this.tabRecordingDevices.Size = new System.Drawing.Size(528, 326);
            this.tabRecordingDevices.TabIndex = 3;
            this.tabRecordingDevices.Text = "Recording devices";
            this.tabRecordingDevices.UseVisualStyleBackColor = true;
            this.tabRecordingDevices.Enter += new System.EventHandler(this.tabDevices_Enter);
            // 
            // tabHotkeys
            // 
            this.tabHotkeys.Controls.Add(this.gridHotkeys);
            this.tabHotkeys.Location = new System.Drawing.Point(4, 22);
            this.tabHotkeys.Margin = new System.Windows.Forms.Padding(2);
            this.tabHotkeys.Name = "tabHotkeys";
            this.tabHotkeys.Padding = new System.Windows.Forms.Padding(2);
            this.tabHotkeys.Size = new System.Drawing.Size(528, 326);
            this.tabHotkeys.TabIndex = 0;
            this.tabHotkeys.Text = "Hot Keys";
            this.tabHotkeys.UseVisualStyleBackColor = true;
            this.tabHotkeys.Enter += new System.EventHandler(this.tabHotkeys_Enter);
            // 
            // gridHotkeys
            // 
            this.gridHotkeys.AllowUserToResizeColumns = false;
            this.gridHotkeys.AllowUserToResizeRows = false;
            this.gridHotkeys.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gridHotkeys.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.gridHotkeys.BackgroundColor = System.Drawing.SystemColors.Window;
            this.gridHotkeys.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.gridHotkeys.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridHotkeys.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Function,
            this.Control,
            this.Alt,
            this.Shift,
            this.LWin,
            this.RWin,
            this.ShowOSD,
            this.HotKey});
            this.gridHotkeys.GridColor = System.Drawing.SystemColors.Control;
            this.gridHotkeys.Location = new System.Drawing.Point(5, 5);
            this.gridHotkeys.Margin = new System.Windows.Forms.Padding(2);
            this.gridHotkeys.Name = "gridHotkeys";
            this.gridHotkeys.RowHeadersWidth = 25;
            this.gridHotkeys.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(186)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            this.gridHotkeys.RowsDefaultCellStyle = dataGridViewCellStyle1;
            this.gridHotkeys.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.gridHotkeys.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.gridHotkeys.Size = new System.Drawing.Size(520, 319);
            this.gridHotkeys.TabIndex = 1;
            this.gridHotkeys.UserAddedRow += new System.Windows.Forms.DataGridViewRowEventHandler(this.gridHotkeys_UserAddedRow);
            this.gridHotkeys.UserDeletingRow += new System.Windows.Forms.DataGridViewRowCancelEventHandler(this.gridHotkeys_UserDeletingRow);
            // 
            // Function
            // 
            this.Function.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.Function.HeaderText = "Function";
            this.Function.MaxDropDownItems = 100;
            this.Function.MinimumWidth = 160;
            this.Function.Name = "Function";
            this.Function.Width = 160;
            // 
            // Control
            // 
            this.Control.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.Control.HeaderText = "Control";
            this.Control.Name = "Control";
            this.Control.Width = 46;
            // 
            // Alt
            // 
            this.Alt.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.Alt.HeaderText = "Alt";
            this.Alt.Name = "Alt";
            this.Alt.Width = 25;
            // 
            // Shift
            // 
            this.Shift.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.Shift.HeaderText = "Shift";
            this.Shift.Name = "Shift";
            this.Shift.Width = 34;
            // 
            // LWin
            // 
            this.LWin.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.LWin.HeaderText = "LWin";
            this.LWin.Name = "LWin";
            this.LWin.Width = 38;
            // 
            // RWin
            // 
            this.RWin.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.RWin.HeaderText = "RWin";
            this.RWin.Name = "RWin";
            this.RWin.Width = 40;
            // 
            // ShowOSD
            // 
            this.ShowOSD.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.ShowOSD.HeaderText = "Show OSD";
            this.ShowOSD.Name = "ShowOSD";
            this.ShowOSD.Width = 66;
            // 
            // HotKey
            // 
            this.HotKey.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.HotKey.HeaderText = "Hot Key";
            this.HotKey.MaxDropDownItems = 20;
            this.HotKey.MinimumWidth = 80;
            this.HotKey.Name = "HotKey";
            // 
            // buttonClose
            // 
            this.buttonClose.Location = new System.Drawing.Point(367, 313);
            this.buttonClose.Margin = new System.Windows.Forms.Padding(2);
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.Size = new System.Drawing.Size(160, 32);
            this.buttonClose.TabIndex = 10;
            this.buttonClose.Text = "Close";
            this.buttonClose.UseVisualStyleBackColor = true;
            this.buttonClose.Click += new System.EventHandler(this.buttonClose_Click);
            // 
            // labelTips
            // 
            this.labelTips.BackColor = System.Drawing.SystemColors.Window;
            this.labelTips.ForeColor = System.Drawing.Color.Green;
            this.labelTips.Location = new System.Drawing.Point(13, 320);
            this.labelTips.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelTips.Name = "labelTips";
            this.labelTips.Size = new System.Drawing.Size(345, 18);
            this.labelTips.TabIndex = 11;
            this.labelTips.Text = "labelTips";
            this.labelTips.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // playbackDevices
            // 
            this.playbackDevices.Location = new System.Drawing.Point(0, 4);
            this.playbackDevices.Name = "playbackDevices";
            this.playbackDevices.Size = new System.Drawing.Size(528, 319);
            this.playbackDevices.TabIndex = 0;
            this.playbackDevices.Load += new System.EventHandler(this.playbackDevices_Load);
            // 
            // recordingDevices
            // 
            this.recordingDevices.Location = new System.Drawing.Point(0, 4);
            this.recordingDevices.Name = "recordingDevices";
            this.recordingDevices.Size = new System.Drawing.Size(528, 319);
            this.recordingDevices.TabIndex = 0;
            this.recordingDevices.Load += new System.EventHandler(this.recordingDevices_Load);
            // 
            // FormSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(538, 356);
            this.Controls.Add(this.labelTips);
            this.Controls.Add(this.buttonClose);
            this.Controls.Add(this.tabControl);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormSettings";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "AudioSwitch - Settings";
            this.TopMost = true;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormSettings_FormClosing);
            this.Load += new System.EventHandler(this.FormSettings_Load);
            this.tabControl.ResumeLayout(false);
            this.tabGeneral.ResumeLayout(false);
            this.tabGeneral.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupOSD.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numTimeout)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackTransparency)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.tabPlaybackDevices.ResumeLayout(false);
            this.tabRecordingDevices.ResumeLayout(false);
            this.tabHotkeys.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridHotkeys)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabHotkeys;
        private System.Windows.Forms.TabPage tabPlaybackDevices;
        private System.Windows.Forms.DataGridView gridHotkeys;
        private System.Windows.Forms.TabPage tabGeneral;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox comboDefMode;
        private System.Windows.Forms.CheckBox checkDefaultMultiAndComm;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.CheckBox checkVolScroll;
        private System.Windows.Forms.CheckBox checkScrShowOSD;
        private System.Windows.Forms.ComboBox comboScrollKey;
        private System.Windows.Forms.Label labelVolScroll;
        private System.Windows.Forms.GroupBox groupOSD;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label labelAuthor;
        private System.Windows.Forms.LinkLabel linkWebpage;
        private System.Windows.Forms.Label labelVersion;
        private System.Windows.Forms.ComboBox comboOSDSkin;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox checkColorVU;
        private System.Windows.Forms.DataGridViewComboBoxColumn Function;
        private System.Windows.Forms.DataGridViewCheckBoxColumn Control;
        private System.Windows.Forms.DataGridViewCheckBoxColumn Alt;
        private System.Windows.Forms.DataGridViewCheckBoxColumn Shift;
        private System.Windows.Forms.DataGridViewCheckBoxColumn LWin;
        private System.Windows.Forms.DataGridViewCheckBoxColumn RWin;
        private System.Windows.Forms.DataGridViewCheckBoxColumn ShowOSD;
        private System.Windows.Forms.DataGridViewComboBoxColumn HotKey;
        private System.Windows.Forms.CheckBox checkShowHWName;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.RadioButton radioQuickSwitch;
        private System.Windows.Forms.RadioButton radioAlwaysMenu;
        private System.Windows.Forms.CheckBox checkQSShowOSD;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TrackBar trackTransparency;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown numTimeout;
        private System.Windows.Forms.CheckBox checkCustomOSD;
        private System.Windows.Forms.CheckBox checkCloseSelect;
        private System.Windows.Forms.TabPage tabRecordingDevices;
        private System.Windows.Forms.Button buttonClose;
        private Devices playbackDevices;
        private Devices recordingDevices;
        private System.Windows.Forms.Label labelTips;
    }
}