using System;
using AudioSwitch.Classes;

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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.tabSettings = new System.Windows.Forms.TabControl();
<<<<<<< HEAD
=======
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
            this.tabDevices = new System.Windows.Forms.TabPage();
            this.label2 = new System.Windows.Forms.Label();
            this.groupDevice = new System.Windows.Forms.GroupBox();
            this.textCustomName = new System.Windows.Forms.TextBox();
            this.checkCustomName = new System.Windows.Forms.CheckBox();
            this.label7 = new System.Windows.Forms.Label();
            this.checkHideDevice = new System.Windows.Forms.CheckBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.buttonResetDevice = new System.Windows.Forms.Button();
            this.buttonSaveDevice = new System.Windows.Forms.Button();
            this.trackBrightness = new System.Windows.Forms.TrackBar();
            this.trackSaturation = new System.Windows.Forms.TrackBar();
            this.trackHue = new System.Windows.Forms.TrackBar();
            this.pictureModded = new System.Windows.Forms.PictureBox();
            this.listDevices = new AudioSwitch.Controls.CustomListView();
>>>>>>> origin/master
            this.tabGeneral = new System.Windows.Forms.TabPage();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.checkQSShowOSD = new System.Windows.Forms.CheckBox();
            this.radioQuickSwitch = new System.Windows.Forms.RadioButton();
            this.radioAlwaysMenu = new System.Windows.Forms.RadioButton();
            this.label5 = new System.Windows.Forms.Label();
            this.checkShowHWName = new System.Windows.Forms.CheckBox();
            this.checkColorVU = new System.Windows.Forms.CheckBox();
            this.label6 = new System.Windows.Forms.Label();
            this.comboDefMode = new System.Windows.Forms.ComboBox();
            this.checkDefaultMultiAndComm = new System.Windows.Forms.CheckBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.checkVolScroll = new System.Windows.Forms.CheckBox();
            this.checkScrShowOSD = new System.Windows.Forms.CheckBox();
            this.comboScrollKey = new System.Windows.Forms.ComboBox();
            this.labelVolScroll = new System.Windows.Forms.Label();
            this.groupOSD = new System.Windows.Forms.GroupBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.labelAuthor = new System.Windows.Forms.Label();
            this.linkWebpage = new System.Windows.Forms.LinkLabel();
            this.labelVersion = new System.Windows.Forms.Label();
            this.comboOSDSkin = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.numTimeout = new System.Windows.Forms.NumericUpDown();
            this.trackTransparency = new System.Windows.Forms.TrackBar();
            this.label3 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.tabDevices = new System.Windows.Forms.TabPage();
            this.label2 = new System.Windows.Forms.Label();
            this.groupDevice = new System.Windows.Forms.GroupBox();
            this.label7 = new System.Windows.Forms.Label();
            this.checkHideDevice = new System.Windows.Forms.CheckBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.buttonResetDevice = new System.Windows.Forms.Button();
            this.buttonSaveDevice = new System.Windows.Forms.Button();
            this.trackBrightness = new System.Windows.Forms.TrackBar();
            this.trackSaturation = new System.Windows.Forms.TrackBar();
            this.trackHue = new System.Windows.Forms.TrackBar();
            this.pictureModded = new System.Windows.Forms.PictureBox();
            this.listDevices = new AudioSwitch.Controls.CustomListView();
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
            this.label4 = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.labelTips = new System.Windows.Forms.Label();
            this.checkCustomOSD = new System.Windows.Forms.CheckBox();
            this.tabSettings.SuspendLayout();
            this.tabGeneral.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupOSD.SuspendLayout();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numTimeout)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackTransparency)).BeginInit();
            this.tabDevices.SuspendLayout();
            this.groupDevice.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBrightness)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackSaturation)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackHue)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureModded)).BeginInit();
            this.tabHotkeys.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridHotkeys)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // tabSettings
            // 
            this.tabSettings.Controls.Add(this.tabGeneral);
            this.tabSettings.Controls.Add(this.tabDevices);
            this.tabSettings.Controls.Add(this.tabHotkeys);
            this.tabSettings.HotTrack = true;
            this.tabSettings.Location = new System.Drawing.Point(3, 4);
            this.tabSettings.Name = "tabSettings";
            this.tabSettings.SelectedIndex = 0;
            this.tabSettings.Size = new System.Drawing.Size(540, 342);
            this.tabSettings.TabIndex = 0;
            // 
            // tabGeneral
            // 
            this.tabGeneral.Controls.Add(this.checkCustomOSD);
            this.tabGeneral.Controls.Add(this.groupBox2);
            this.tabGeneral.Controls.Add(this.groupBox4);
            this.tabGeneral.Controls.Add(this.groupOSD);
            this.tabGeneral.Location = new System.Drawing.Point(4, 22);
            this.tabGeneral.Name = "tabGeneral";
            this.tabGeneral.Size = new System.Drawing.Size(532, 316);
            this.tabGeneral.TabIndex = 2;
            this.tabGeneral.Text = "General";
            this.tabGeneral.UseVisualStyleBackColor = true;
            this.tabGeneral.Enter += new System.EventHandler(this.tabOSD_Enter);
            this.tabGeneral.Leave += new System.EventHandler(this.tabOSD_Leave);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.checkQSShowOSD);
            this.groupBox2.Controls.Add(this.radioQuickSwitch);
            this.groupBox2.Controls.Add(this.radioAlwaysMenu);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.checkShowHWName);
            this.groupBox2.Controls.Add(this.checkColorVU);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.comboDefMode);
            this.groupBox2.Controls.Add(this.checkDefaultMultiAndComm);
            this.groupBox2.Location = new System.Drawing.Point(9, 194);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(510, 114);
            this.groupBox2.TabIndex = 21;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "General Behavior";
            // 
            // checkQSShowOSD
            // 
            this.checkQSShowOSD.AutoSize = true;
            this.checkQSShowOSD.Location = new System.Drawing.Point(334, 90);
            this.checkQSShowOSD.Name = "checkQSShowOSD";
            this.checkQSShowOSD.Size = new System.Drawing.Size(79, 17);
            this.checkQSShowOSD.TabIndex = 26;
            this.checkQSShowOSD.Text = "Show OSD";
            this.checkQSShowOSD.UseVisualStyleBackColor = true;
            // 
            // radioQuickSwitch
            // 
            this.radioQuickSwitch.Location = new System.Drawing.Point(315, 54);
            this.radioQuickSwitch.Name = "radioQuickSwitch";
            this.radioQuickSwitch.Size = new System.Drawing.Size(187, 33);
            this.radioQuickSwitch.TabIndex = 24;
            this.radioQuickSwitch.Text = "menu when AudioSwitch is open, otherwise quick-switches device";
            this.radioQuickSwitch.UseVisualStyleBackColor = true;
            this.radioQuickSwitch.CheckedChanged += new System.EventHandler(this.radioQuickSwitch_CheckedChanged);
            // 
            // radioAlwaysMenu
            // 
            this.radioAlwaysMenu.Checked = true;
            this.radioAlwaysMenu.Location = new System.Drawing.Point(315, 34);
            this.radioAlwaysMenu.Name = "radioAlwaysMenu";
            this.radioAlwaysMenu.Size = new System.Drawing.Size(86, 17);
            this.radioAlwaysMenu.TabIndex = 23;
            this.radioAlwaysMenu.TabStop = true;
            this.radioAlwaysMenu.Text = "always menu";
            this.radioAlwaysMenu.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(304, 16);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(149, 13);
            this.label5.TabIndex = 25;
            this.label5.Text = "Right-clicking tray icon opens:";
            // 
            // checkShowHWName
            // 
            this.checkShowHWName.AutoSize = true;
            this.checkShowHWName.Location = new System.Drawing.Point(14, 69);
            this.checkShowHWName.Name = "checkShowHWName";
            this.checkShowHWName.Size = new System.Drawing.Size(186, 17);
            this.checkShowHWName.TabIndex = 22;
            this.checkShowHWName.Text = "Show also device hardware name";
            this.checkShowHWName.UseVisualStyleBackColor = true;
            // 
            // checkColorVU
            // 
            this.checkColorVU.Location = new System.Drawing.Point(14, 91);
            this.checkColorVU.Name = "checkColorVU";
            this.checkColorVU.Size = new System.Drawing.Size(121, 17);
            this.checkColorVU.TabIndex = 21;
            this.checkColorVU.Text = "Color LED VU meter";
            this.checkColorVU.UseVisualStyleBackColor = true;
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(11, 24);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(108, 13);
            this.label6.TabIndex = 18;
            this.label6.Text = "Default GUI Devices:";
            // 
            // comboDefMode
            // 
            this.comboDefMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboDefMode.FormattingEnabled = true;
            this.comboDefMode.Location = new System.Drawing.Point(125, 21);
            this.comboDefMode.Name = "comboDefMode";
            this.comboDefMode.Size = new System.Drawing.Size(121, 21);
            this.comboDefMode.TabIndex = 19;
            // 
            // checkDefaultMultiAndComm
            // 
            this.checkDefaultMultiAndComm.Location = new System.Drawing.Point(14, 47);
            this.checkDefaultMultiAndComm.Name = "checkDefaultMultiAndComm";
            this.checkDefaultMultiAndComm.Size = new System.Drawing.Size(232, 17);
            this.checkDefaultMultiAndComm.TabIndex = 20;
            this.checkDefaultMultiAndComm.Text = "Switch also default communications device";
            this.checkDefaultMultiAndComm.UseVisualStyleBackColor = true;
            this.checkDefaultMultiAndComm.CheckedChanged += new System.EventHandler(this.checkDefaultMultiAndComm_CheckedChanged);
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.checkVolScroll);
            this.groupBox4.Controls.Add(this.checkScrShowOSD);
            this.groupBox4.Controls.Add(this.comboScrollKey);
            this.groupBox4.Controls.Add(this.labelVolScroll);
            this.groupBox4.Location = new System.Drawing.Point(9, 135);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(510, 53);
            this.groupBox4.TabIndex = 8;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Volume Scrolling";
            // 
            // checkVolScroll
            // 
            this.checkVolScroll.Location = new System.Drawing.Point(14, 23);
            this.checkVolScroll.Name = "checkVolScroll";
            this.checkVolScroll.Size = new System.Drawing.Size(65, 17);
            this.checkVolScroll.TabIndex = 22;
            this.checkVolScroll.Text = "Enabled";
            this.checkVolScroll.UseVisualStyleBackColor = true;
            this.checkVolScroll.CheckedChanged += new System.EventHandler(this.checkVolScroll_CheckedChanged);
            // 
            // checkScrShowOSD
            // 
            this.checkScrShowOSD.Location = new System.Drawing.Point(390, 23);
            this.checkScrShowOSD.Name = "checkScrShowOSD";
            this.checkScrShowOSD.Size = new System.Drawing.Size(79, 17);
            this.checkScrShowOSD.TabIndex = 8;
            this.checkScrShowOSD.Text = "Show OSD";
            this.checkScrShowOSD.UseVisualStyleBackColor = true;
            this.checkScrShowOSD.CheckedChanged += new System.EventHandler(this.checkScrShowOSD_CheckedChanged);
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
            "Shift"});
            this.comboScrollKey.Location = new System.Drawing.Point(85, 21);
            this.comboScrollKey.Name = "comboScrollKey";
            this.comboScrollKey.Size = new System.Drawing.Size(161, 21);
            this.comboScrollKey.TabIndex = 21;
            this.comboScrollKey.SelectedIndexChanged += new System.EventHandler(this.comboScrollKey_SelectedIndexChanged);
            // 
            // labelVolScroll
            // 
            this.labelVolScroll.Location = new System.Drawing.Point(245, 24);
            this.labelVolScroll.Name = "labelVolScroll";
            this.labelVolScroll.Size = new System.Drawing.Size(85, 13);
            this.labelVolScroll.TabIndex = 23;
            this.labelVolScroll.Text = " + Mouse Wheel";
            // 
            // groupOSD
            // 
            this.groupOSD.Controls.Add(this.label3);
            this.groupOSD.Controls.Add(this.groupBox3);
            this.groupOSD.Controls.Add(this.numTimeout);
            this.groupOSD.Controls.Add(this.trackTransparency);
            this.groupOSD.Controls.Add(this.label8);
            this.groupOSD.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(186)));
            this.groupOSD.Location = new System.Drawing.Point(9, 9);
            this.groupOSD.Name = "groupOSD";
            this.groupOSD.Size = new System.Drawing.Size(510, 120);
            this.groupOSD.TabIndex = 18;
            this.groupOSD.TabStop = false;
            this.groupOSD.Text = "OSD Settings";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.labelAuthor);
            this.groupBox3.Controls.Add(this.linkWebpage);
            this.groupBox3.Controls.Add(this.labelVersion);
            this.groupBox3.Controls.Add(this.comboOSDSkin);
            this.groupBox3.Controls.Add(this.label1);
            this.groupBox3.Location = new System.Drawing.Point(8, 16);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(273, 99);
            this.groupBox3.TabIndex = 8;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Skin";
            // 
            // labelAuthor
            // 
            this.labelAuthor.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(186)));
            this.labelAuthor.Location = new System.Drawing.Point(15, 47);
            this.labelAuthor.Name = "labelAuthor";
            this.labelAuthor.Size = new System.Drawing.Size(208, 13);
            this.labelAuthor.TabIndex = 8;
            this.labelAuthor.Text = "<Author>";
            this.labelAuthor.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // linkWebpage
            // 
            this.linkWebpage.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.linkWebpage.LinkColor = System.Drawing.SystemColors.HotTrack;
            this.linkWebpage.Location = new System.Drawing.Point(15, 75);
            this.linkWebpage.Name = "linkWebpage";
            this.linkWebpage.Size = new System.Drawing.Size(208, 13);
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
            this.labelVersion.Location = new System.Drawing.Point(54, 60);
            this.labelVersion.Name = "labelVersion";
            this.labelVersion.Size = new System.Drawing.Size(29, 12);
            this.labelVersion.TabIndex = 16;
            this.labelVersion.Text = "<ver>";
            // 
            // comboOSDSkin
            // 
            this.comboOSDSkin.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboOSDSkin.FormattingEnabled = true;
            this.comboOSDSkin.Location = new System.Drawing.Point(18, 22);
            this.comboOSDSkin.Name = "comboOSDSkin";
            this.comboOSDSkin.Size = new System.Drawing.Size(205, 21);
            this.comboOSDSkin.TabIndex = 12;
            this.comboOSDSkin.SelectedIndexChanged += new System.EventHandler(this.comboOSDSkin_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(186)));
            this.label1.Location = new System.Drawing.Point(16, 60);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(37, 12);
            this.label1.TabIndex = 8;
            this.label1.Text = "Version";
            // 
            // numTimeout
            // 
            this.numTimeout.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.numTimeout.Location = new System.Drawing.Point(424, 90);
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
            this.numTimeout.Size = new System.Drawing.Size(56, 20);
            this.numTimeout.TabIndex = 10;
            this.numTimeout.Value = new decimal(new int[] {
            2000,
            0,
            0,
            0});
            this.numTimeout.ValueChanged += new System.EventHandler(this.numTimeout_ValueChanged);
            // 
            // trackTransparency
            // 
            this.trackTransparency.BackColor = System.Drawing.SystemColors.Window;
            this.trackTransparency.Location = new System.Drawing.Point(320, 61);
            this.trackTransparency.Maximum = 255;
            this.trackTransparency.Name = "trackTransparency";
            this.trackTransparency.Size = new System.Drawing.Size(160, 45);
            this.trackTransparency.TabIndex = 14;
            this.trackTransparency.TickStyle = System.Windows.Forms.TickStyle.None;
            this.trackTransparency.Value = 255;
            this.trackTransparency.Scroll += new System.EventHandler(this.trackTransparency_ValueChanged);
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(311, 92);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(107, 13);
            this.label3.TabIndex = 11;
            this.label3.Text = "Closing Timeout (ms):";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label8
            // 
            this.label8.Location = new System.Drawing.Point(317, 42);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(72, 13);
            this.label8.TabIndex = 8;
            this.label8.Text = "Transparency";
            // 
            // tabDevices
            // 
            this.tabDevices.Controls.Add(this.label2);
            this.tabDevices.Controls.Add(this.groupDevice);
            this.tabDevices.Controls.Add(this.listDevices);
            this.tabDevices.Location = new System.Drawing.Point(4, 22);
            this.tabDevices.Name = "tabDevices";
            this.tabDevices.Padding = new System.Windows.Forms.Padding(3);
            this.tabDevices.Size = new System.Drawing.Size(532, 316);
            this.tabDevices.TabIndex = 1;
            this.tabDevices.Text = "Devices";
            this.tabDevices.UseVisualStyleBackColor = true;
            this.tabDevices.Enter += new System.EventHandler(this.tabDevices_Enter);
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(6, 6);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(101, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "Connected Devices";
            // 
            // groupDevice
            // 
            this.groupDevice.Controls.Add(this.textCustomName);
            this.groupDevice.Controls.Add(this.checkCustomName);
            this.groupDevice.Controls.Add(this.label7);
            this.groupDevice.Controls.Add(this.checkHideDevice);
            this.groupDevice.Controls.Add(this.label9);
            this.groupDevice.Controls.Add(this.label10);
            this.groupDevice.Controls.Add(this.label11);
            this.groupDevice.Controls.Add(this.buttonResetDevice);
            this.groupDevice.Controls.Add(this.buttonSaveDevice);
            this.groupDevice.Controls.Add(this.trackBrightness);
            this.groupDevice.Controls.Add(this.trackSaturation);
            this.groupDevice.Controls.Add(this.trackHue);
            this.groupDevice.Controls.Add(this.pictureModded);
            this.groupDevice.Location = new System.Drawing.Point(268, 6);
            this.groupDevice.Name = "groupDevice";
            this.groupDevice.Size = new System.Drawing.Size(254, 304);
            this.groupDevice.TabIndex = 8;
            this.groupDevice.TabStop = false;
            this.groupDevice.Text = "Selected Device Settings";
            // 
            // textCustomName
            // 
            this.textCustomName.Enabled = false;
            this.textCustomName.Location = new System.Drawing.Point(16, 187);
            this.textCustomName.Name = "textCustomName";
            this.textCustomName.Size = new System.Drawing.Size(223, 20);
            this.textCustomName.TabIndex = 4;
            // 
            // checkCustomName
            // 
            this.checkCustomName.Location = new System.Drawing.Point(17, 163);
            this.checkCustomName.Name = "checkCustomName";
            this.checkCustomName.Size = new System.Drawing.Size(154, 17);
            this.checkCustomName.TabIndex = 3;
            this.checkCustomName.Text = "Customize display name";
            this.checkCustomName.UseVisualStyleBackColor = true;
            this.checkCustomName.CheckedChanged += new System.EventHandler(this.checkCustomName_CheckedChanged);
            // 
            // label7
            // 
            this.label7.Location = new System.Drawing.Point(121, 27);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(79, 13);
            this.label7.TabIndex = 26;
            this.label7.Text = "Tray Icon Color";
            // 
            // checkHideDevice
            // 
            this.checkHideDevice.Location = new System.Drawing.Point(17, 237);
            this.checkHideDevice.Name = "checkHideDevice";
            this.checkHideDevice.Size = new System.Drawing.Size(154, 17);
            this.checkHideDevice.TabIndex = 5;
            this.checkHideDevice.Text = "Hide device from switch list";
            this.checkHideDevice.UseVisualStyleBackColor = true;
            // 
            // label9
            // 
            this.label9.Location = new System.Drawing.Point(42, 63);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(27, 13);
            this.label9.TabIndex = 16;
            this.label9.Text = "Hue";
            // 
            // label10
            // 
            this.label10.Location = new System.Drawing.Point(14, 88);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(55, 13);
            this.label10.TabIndex = 19;
            this.label10.Text = "Saturation";
            // 
            // label11
            // 
            this.label11.Location = new System.Drawing.Point(13, 114);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(56, 13);
            this.label11.TabIndex = 21;
            this.label11.Text = "Brightness";
            // 
            // buttonResetDevice
            // 
            this.buttonResetDevice.Location = new System.Drawing.Point(16, 263);
            this.buttonResetDevice.Name = "buttonResetDevice";
            this.buttonResetDevice.Size = new System.Drawing.Size(102, 27);
            this.buttonResetDevice.TabIndex = 6;
            this.buttonResetDevice.Text = "Remove Settings";
            this.buttonResetDevice.UseVisualStyleBackColor = true;
            this.buttonResetDevice.Click += new System.EventHandler(this.buttonResetDevice_Click);
            // 
            // buttonSaveDevice
            // 
            this.buttonSaveDevice.Location = new System.Drawing.Point(137, 263);
            this.buttonSaveDevice.Name = "buttonSaveDevice";
            this.buttonSaveDevice.Size = new System.Drawing.Size(102, 27);
            this.buttonSaveDevice.TabIndex = 7;
            this.buttonSaveDevice.Text = "Save Settings";
            this.buttonSaveDevice.UseVisualStyleBackColor = true;
            this.buttonSaveDevice.Click += new System.EventHandler(this.buttonSaveDevice_Click);
            // 
            // trackBrightness
            // 
            this.trackBrightness.BackColor = System.Drawing.SystemColors.Window;
            this.trackBrightness.Location = new System.Drawing.Point(75, 112);
            this.trackBrightness.Maximum = 60;
            this.trackBrightness.Name = "trackBrightness";
            this.trackBrightness.Size = new System.Drawing.Size(164, 45);
            this.trackBrightness.TabIndex = 2;
            this.trackBrightness.TickStyle = System.Windows.Forms.TickStyle.None;
            this.trackBrightness.Scroll += new System.EventHandler(this.trackBarsHSB_Scroll);
            // 
            // trackSaturation
            // 
            this.trackSaturation.BackColor = System.Drawing.SystemColors.Window;
            this.trackSaturation.Location = new System.Drawing.Point(75, 86);
            this.trackSaturation.Maximum = 100;
            this.trackSaturation.Name = "trackSaturation";
            this.trackSaturation.Size = new System.Drawing.Size(164, 45);
            this.trackSaturation.TabIndex = 1;
            this.trackSaturation.TickStyle = System.Windows.Forms.TickStyle.None;
            this.trackSaturation.Scroll += new System.EventHandler(this.trackBarsHSB_Scroll);
            // 
            // trackHue
<<<<<<< HEAD
=======
            // 
            this.trackHue.BackColor = System.Drawing.SystemColors.Window;
            this.trackHue.Location = new System.Drawing.Point(75, 61);
            this.trackHue.Maximum = 360;
            this.trackHue.Name = "trackHue";
            this.trackHue.Size = new System.Drawing.Size(164, 45);
            this.trackHue.TabIndex = 0;
            this.trackHue.TickStyle = System.Windows.Forms.TickStyle.None;
            this.trackHue.Scroll += new System.EventHandler(this.trackBarsHSB_Scroll);
            // 
            // pictureModded
            // 
            this.pictureModded.Location = new System.Drawing.Point(209, 19);
            this.pictureModded.Name = "pictureModded";
            this.pictureModded.Size = new System.Drawing.Size(30, 30);
            this.pictureModded.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureModded.TabIndex = 20;
            this.pictureModded.TabStop = false;
            // 
            // listDevices
            // 
            this.listDevices.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listDevices.BackColor = System.Drawing.SystemColors.Window;
            this.listDevices.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.listDevices.FullRowSelect = true;
            this.listDevices.HideSelection = false;
            this.listDevices.Location = new System.Drawing.Point(6, 25);
            this.listDevices.MultiSelect = false;
            this.listDevices.Name = "listDevices";
            this.listDevices.Size = new System.Drawing.Size(256, 285);
            this.listDevices.TabIndex = 0;
            this.listDevices.TileSize = new System.Drawing.Size(238, 40);
            this.listDevices.UseCompatibleStateImageBehavior = false;
            this.listDevices.View = System.Windows.Forms.View.Tile;
            this.listDevices.SelectedIndexChanged += new System.EventHandler(this.listDevices_SelectedIndexChanged);
            // 
            // tabGeneral
            // 
            this.tabGeneral.Controls.Add(this.groupBox2);
            this.tabGeneral.Controls.Add(this.groupBox4);
            this.tabGeneral.Controls.Add(this.groupBox1);
            this.tabGeneral.Location = new System.Drawing.Point(4, 22);
            this.tabGeneral.Name = "tabGeneral";
            this.tabGeneral.Size = new System.Drawing.Size(532, 316);
            this.tabGeneral.TabIndex = 2;
            this.tabGeneral.Text = "General";
            this.tabGeneral.UseVisualStyleBackColor = true;
            this.tabGeneral.Enter += new System.EventHandler(this.tabOSD_Enter);
            this.tabGeneral.Leave += new System.EventHandler(this.tabOSD_Leave);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.checkQSShowOSD);
            this.groupBox2.Controls.Add(this.radioQuickSwitch);
            this.groupBox2.Controls.Add(this.radioAlwaysMenu);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.checkShowHWName);
            this.groupBox2.Controls.Add(this.checkColorVU);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.comboDefMode);
            this.groupBox2.Controls.Add(this.checkDefaultMultiAndComm);
            this.groupBox2.Location = new System.Drawing.Point(9, 194);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(510, 114);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "General Behavior";
            // 
            // checkQSShowOSD
            // 
            this.checkQSShowOSD.AutoSize = true;
            this.checkQSShowOSD.Location = new System.Drawing.Point(334, 90);
            this.checkQSShowOSD.Name = "checkQSShowOSD";
            this.checkQSShowOSD.Size = new System.Drawing.Size(79, 17);
            this.checkQSShowOSD.TabIndex = 6;
            this.checkQSShowOSD.Text = "Show OSD";
            this.checkQSShowOSD.UseVisualStyleBackColor = true;
            // 
            // radioQuickSwitch
            // 
            this.radioQuickSwitch.Location = new System.Drawing.Point(315, 54);
            this.radioQuickSwitch.Name = "radioQuickSwitch";
            this.radioQuickSwitch.Size = new System.Drawing.Size(187, 33);
            this.radioQuickSwitch.TabIndex = 5;
            this.radioQuickSwitch.Text = "menu when AudioSwitch is open, otherwise quick-switches device";
            this.radioQuickSwitch.UseVisualStyleBackColor = true;
            this.radioQuickSwitch.CheckedChanged += new System.EventHandler(this.radioQuickSwitch_CheckedChanged);
            // 
            // radioAlwaysMenu
            // 
            this.radioAlwaysMenu.Checked = true;
            this.radioAlwaysMenu.Location = new System.Drawing.Point(315, 34);
            this.radioAlwaysMenu.Name = "radioAlwaysMenu";
            this.radioAlwaysMenu.Size = new System.Drawing.Size(86, 17);
            this.radioAlwaysMenu.TabIndex = 4;
            this.radioAlwaysMenu.TabStop = true;
            this.radioAlwaysMenu.Text = "always menu";
            this.radioAlwaysMenu.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(304, 16);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(149, 13);
            this.label5.TabIndex = 25;
            this.label5.Text = "Right-clicking tray icon opens:";
            // 
            // checkShowHWName
            // 
            this.checkShowHWName.AutoSize = true;
            this.checkShowHWName.Location = new System.Drawing.Point(14, 69);
            this.checkShowHWName.Name = "checkShowHWName";
            this.checkShowHWName.Size = new System.Drawing.Size(186, 17);
            this.checkShowHWName.TabIndex = 2;
            this.checkShowHWName.Text = "Show also device hardware name";
            this.checkShowHWName.UseVisualStyleBackColor = true;
            // 
            // checkColorVU
            // 
            this.checkColorVU.Location = new System.Drawing.Point(14, 91);
            this.checkColorVU.Name = "checkColorVU";
            this.checkColorVU.Size = new System.Drawing.Size(121, 17);
            this.checkColorVU.TabIndex = 3;
            this.checkColorVU.Text = "Color LED VU meter";
            this.checkColorVU.UseVisualStyleBackColor = true;
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(11, 24);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(108, 13);
            this.label6.TabIndex = 18;
            this.label6.Text = "Default GUI Devices:";
            // 
            // comboDefMode
            // 
            this.comboDefMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboDefMode.FormattingEnabled = true;
            this.comboDefMode.Location = new System.Drawing.Point(125, 21);
            this.comboDefMode.Name = "comboDefMode";
            this.comboDefMode.Size = new System.Drawing.Size(121, 21);
            this.comboDefMode.TabIndex = 0;
            // 
            // checkDefaultMultiAndComm
            // 
            this.checkDefaultMultiAndComm.Location = new System.Drawing.Point(14, 47);
            this.checkDefaultMultiAndComm.Name = "checkDefaultMultiAndComm";
            this.checkDefaultMultiAndComm.Size = new System.Drawing.Size(232, 17);
            this.checkDefaultMultiAndComm.TabIndex = 1;
            this.checkDefaultMultiAndComm.Text = "Switch also default communications device";
            this.checkDefaultMultiAndComm.UseVisualStyleBackColor = true;
            this.checkDefaultMultiAndComm.CheckedChanged += new System.EventHandler(this.checkDefaultMultiAndComm_CheckedChanged);
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.checkVolScroll);
            this.groupBox4.Controls.Add(this.checkScrShowOSD);
            this.groupBox4.Controls.Add(this.comboScrollKey);
            this.groupBox4.Controls.Add(this.labelVolScroll);
            this.groupBox4.Location = new System.Drawing.Point(9, 135);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(510, 53);
            this.groupBox4.TabIndex = 1;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Volume Scrolling";
            // 
            // checkVolScroll
            // 
            this.checkVolScroll.Location = new System.Drawing.Point(14, 23);
            this.checkVolScroll.Name = "checkVolScroll";
            this.checkVolScroll.Size = new System.Drawing.Size(65, 17);
            this.checkVolScroll.TabIndex = 0;
            this.checkVolScroll.Text = "Enabled";
            this.checkVolScroll.UseVisualStyleBackColor = true;
            this.checkVolScroll.CheckedChanged += new System.EventHandler(this.checkVolScroll_CheckedChanged);
            // 
            // checkScrShowOSD
            // 
            this.checkScrShowOSD.Location = new System.Drawing.Point(390, 23);
            this.checkScrShowOSD.Name = "checkScrShowOSD";
            this.checkScrShowOSD.Size = new System.Drawing.Size(79, 17);
            this.checkScrShowOSD.TabIndex = 2;
            this.checkScrShowOSD.Text = "Show OSD";
            this.checkScrShowOSD.UseVisualStyleBackColor = true;
            this.checkScrShowOSD.CheckedChanged += new System.EventHandler(this.checkScrShowOSD_CheckedChanged);
>>>>>>> origin/master
            // 
            this.trackHue.BackColor = System.Drawing.SystemColors.Window;
            this.trackHue.Location = new System.Drawing.Point(75, 61);
            this.trackHue.Maximum = 360;
            this.trackHue.Name = "trackHue";
            this.trackHue.Size = new System.Drawing.Size(164, 45);
            this.trackHue.TabIndex = 22;
            this.trackHue.TickStyle = System.Windows.Forms.TickStyle.None;
            this.trackHue.Scroll += new System.EventHandler(this.trackBarsHSB_Scroll);
            // 
<<<<<<< HEAD
            // pictureModded
=======
            this.comboScrollKey.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboScrollKey.FormattingEnabled = true;
            this.comboScrollKey.Items.AddRange(new object[] {
            "LeftMouseButton",
            "RightMouseButton",
            "Control",
            "Alt",
            "LWin",
            "RWin",
            "Shift"});
            this.comboScrollKey.Location = new System.Drawing.Point(85, 21);
            this.comboScrollKey.Name = "comboScrollKey";
            this.comboScrollKey.Size = new System.Drawing.Size(161, 21);
            this.comboScrollKey.TabIndex = 1;
            this.comboScrollKey.SelectedIndexChanged += new System.EventHandler(this.comboScrollKey_SelectedIndexChanged);
>>>>>>> origin/master
            // 
            this.pictureModded.Location = new System.Drawing.Point(209, 19);
            this.pictureModded.Name = "pictureModded";
            this.pictureModded.Size = new System.Drawing.Size(30, 30);
            this.pictureModded.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureModded.TabIndex = 20;
            this.pictureModded.TabStop = false;
            // 
            // listDevices
            // 
<<<<<<< HEAD
            this.listDevices.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listDevices.BackColor = System.Drawing.SystemColors.Window;
            this.listDevices.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.listDevices.FullRowSelect = true;
            this.listDevices.HideSelection = false;
            this.listDevices.Location = new System.Drawing.Point(6, 25);
            this.listDevices.MultiSelect = false;
            this.listDevices.Name = "listDevices";
            this.listDevices.Size = new System.Drawing.Size(256, 285);
            this.listDevices.TabIndex = 1;
            this.listDevices.TileSize = new System.Drawing.Size(238, 40);
            this.listDevices.UseCompatibleStateImageBehavior = false;
            this.listDevices.View = System.Windows.Forms.View.Tile;
            this.listDevices.SelectedIndexChanged += new System.EventHandler(this.listDevices_SelectedIndexChanged);
=======
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.groupBox3);
            this.groupBox1.Controls.Add(this.numTimeout);
            this.groupBox1.Controls.Add(this.trackTransparency);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(186)));
            this.groupBox1.Location = new System.Drawing.Point(9, 9);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(510, 120);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "OSD Settings";
>>>>>>> origin/master
            // 
            // tabHotkeys
            // 
<<<<<<< HEAD
            this.tabHotkeys.Controls.Add(this.gridHotkeys);
            this.tabHotkeys.Location = new System.Drawing.Point(4, 22);
            this.tabHotkeys.Name = "tabHotkeys";
            this.tabHotkeys.Padding = new System.Windows.Forms.Padding(3);
            this.tabHotkeys.Size = new System.Drawing.Size(532, 316);
            this.tabHotkeys.TabIndex = 0;
            this.tabHotkeys.Text = "Hot Keys";
            this.tabHotkeys.UseVisualStyleBackColor = true;
            this.tabHotkeys.Enter += new System.EventHandler(this.tabHotkeys_Enter);
=======
            this.groupBox3.Controls.Add(this.labelAuthor);
            this.groupBox3.Controls.Add(this.linkWebpage);
            this.groupBox3.Controls.Add(this.labelVersion);
            this.groupBox3.Controls.Add(this.comboOSDSkin);
            this.groupBox3.Controls.Add(this.label1);
            this.groupBox3.Location = new System.Drawing.Point(8, 16);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(273, 99);
            this.groupBox3.TabIndex = 0;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Skin";
>>>>>>> origin/master
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
            this.gridHotkeys.Location = new System.Drawing.Point(6, 6);
            this.gridHotkeys.Name = "gridHotkeys";
            this.gridHotkeys.RowHeadersWidth = 25;
            this.gridHotkeys.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(186)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            this.gridHotkeys.RowsDefaultCellStyle = dataGridViewCellStyle2;
            this.gridHotkeys.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.gridHotkeys.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.gridHotkeys.Size = new System.Drawing.Size(520, 304);
            this.gridHotkeys.TabIndex = 1;
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
<<<<<<< HEAD
            this.Alt.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.Alt.HeaderText = "Alt";
            this.Alt.Name = "Alt";
            this.Alt.Width = 25;
=======
            this.comboOSDSkin.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboOSDSkin.FormattingEnabled = true;
            this.comboOSDSkin.Location = new System.Drawing.Point(18, 22);
            this.comboOSDSkin.Name = "comboOSDSkin";
            this.comboOSDSkin.Size = new System.Drawing.Size(205, 21);
            this.comboOSDSkin.TabIndex = 0;
            this.comboOSDSkin.SelectedIndexChanged += new System.EventHandler(this.comboOSDSkin_SelectedIndexChanged);
>>>>>>> origin/master
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
<<<<<<< HEAD
            this.LWin.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.LWin.HeaderText = "LWin";
            this.LWin.Name = "LWin";
            this.LWin.Width = 38;
=======
            this.numTimeout.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.numTimeout.Location = new System.Drawing.Point(421, 79);
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
            this.numTimeout.Size = new System.Drawing.Size(56, 20);
            this.numTimeout.TabIndex = 2;
            this.numTimeout.Value = new decimal(new int[] {
            2000,
            0,
            0,
            0});
            this.numTimeout.ValueChanged += new System.EventHandler(this.numTimeout_ValueChanged);
>>>>>>> origin/master
            // 
            // RWin
            // 
<<<<<<< HEAD
            this.RWin.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.RWin.HeaderText = "RWin";
            this.RWin.Name = "RWin";
            this.RWin.Width = 40;
=======
            this.trackTransparency.BackColor = System.Drawing.SystemColors.Window;
            this.trackTransparency.Location = new System.Drawing.Point(317, 37);
            this.trackTransparency.Maximum = 255;
            this.trackTransparency.Name = "trackTransparency";
            this.trackTransparency.Size = new System.Drawing.Size(160, 45);
            this.trackTransparency.TabIndex = 1;
            this.trackTransparency.Value = 255;
            this.trackTransparency.Scroll += new System.EventHandler(this.trackTransparency_ValueChanged);
>>>>>>> origin/master
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
            this.buttonClose.Location = new System.Drawing.Point(410, 347);
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.Size = new System.Drawing.Size(132, 30);
            this.buttonClose.TabIndex = 4;
            this.buttonClose.Text = "Close";
            this.buttonClose.UseVisualStyleBackColor = true;
            this.buttonClose.Click += new System.EventHandler(this.buttonClose_Click);
            // 
            // label4
            // 
            this.label4.BackColor = System.Drawing.Color.Transparent;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(186)));
            this.label4.Location = new System.Drawing.Point(37, 351);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(130, 20);
            this.label4.TabIndex = 5;
            this.label4.Text = "AudioSwitch v2.1";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.Color.Transparent;
            this.pictureBox1.Image = global::AudioSwitch.Properties.Resources.spkr;
            this.pictureBox1.Location = new System.Drawing.Point(3, 346);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(37, 34);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 6;
            this.pictureBox1.TabStop = false;
            // 
            // labelTips
            // 
            this.labelTips.ForeColor = System.Drawing.Color.Green;
            this.labelTips.Location = new System.Drawing.Point(170, 4);
            this.labelTips.Name = "labelTips";
            this.labelTips.Size = new System.Drawing.Size(367, 16);
            this.labelTips.TabIndex = 7;
            this.labelTips.Text = "labelTips";
            this.labelTips.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // checkCustomOSD
            // 
            this.checkCustomOSD.AutoSize = true;
            this.checkCustomOSD.Checked = true;
            this.checkCustomOSD.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkCustomOSD.Location = new System.Drawing.Point(383, 24);
            this.checkCustomOSD.Name = "checkCustomOSD";
            this.checkCustomOSD.Size = new System.Drawing.Size(127, 17);
            this.checkCustomOSD.TabIndex = 15;
            this.checkCustomOSD.Text = "Use customized OSD";
            this.checkCustomOSD.UseVisualStyleBackColor = true;
            this.checkCustomOSD.CheckedChanged += new System.EventHandler(this.checkCustomOSD_CheckedChanged);
            // 
            // FormSettings
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(547, 380);
            this.Controls.Add(this.labelTips);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.buttonClose);
            this.Controls.Add(this.tabSettings);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
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
            this.tabSettings.ResumeLayout(false);
            this.tabGeneral.ResumeLayout(false);
            this.tabGeneral.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupOSD.ResumeLayout(false);
            this.groupOSD.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.numTimeout)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackTransparency)).EndInit();
            this.tabDevices.ResumeLayout(false);
            this.groupDevice.ResumeLayout(false);
            this.groupDevice.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBrightness)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackSaturation)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackHue)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureModded)).EndInit();
            this.tabHotkeys.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridHotkeys)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabSettings;
        private System.Windows.Forms.TabPage tabHotkeys;
        private System.Windows.Forms.TabPage tabDevices;
        private System.Windows.Forms.DataGridView gridHotkeys;
        private System.Windows.Forms.Button buttonClose;
        private Controls.CustomListView listDevices;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.GroupBox groupDevice;
        private System.Windows.Forms.CheckBox checkHideDevice;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Button buttonResetDevice;
        private System.Windows.Forms.Button buttonSaveDevice;
        private System.Windows.Forms.TrackBar trackBrightness;
        private System.Windows.Forms.TrackBar trackSaturation;
        private System.Windows.Forms.TrackBar trackHue;
        private System.Windows.Forms.PictureBox pictureModded;
        private System.Windows.Forms.Label label7;
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
        private System.Windows.Forms.NumericUpDown numTimeout;
        private System.Windows.Forms.TrackBar trackTransparency;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label2;
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
        private System.Windows.Forms.Label labelTips;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.RadioButton radioQuickSwitch;
        private System.Windows.Forms.RadioButton radioAlwaysMenu;
        private System.Windows.Forms.CheckBox checkQSShowOSD;
<<<<<<< HEAD
        private System.Windows.Forms.CheckBox checkCustomOSD;
=======
        private System.Windows.Forms.CheckBox checkCustomName;
        private System.Windows.Forms.TextBox textCustomName;
>>>>>>> origin/master
    }
}