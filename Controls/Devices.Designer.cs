namespace AudioSwitch.Forms
{
    partial class Devices
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.listDevices = new AudioSwitch.Controls.CustomListView();
            this.label4 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.checkEnableAsCommunicationsOnStartup = new System.Windows.Forms.CheckBox();
            this.checkEnableAsMultimediaOnStartup = new System.Windows.Forms.CheckBox();
            this.checkCustomName = new System.Windows.Forms.CheckBox();
            this.pictureModded = new System.Windows.Forms.PictureBox();
            this.textCustomName = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.checkHideDevice = new System.Windows.Forms.CheckBox();
            this.label13 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.buttonRemove = new System.Windows.Forms.Button();
            this.buttonSave = new System.Windows.Forms.Button();
            this.trackBrightness = new System.Windows.Forms.TrackBar();
            this.trackSaturation = new System.Windows.Forms.TrackBar();
            this.trackHue = new System.Windows.Forms.TrackBar();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureModded)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBrightness)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackSaturation)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackHue)).BeginInit();
            this.SuspendLayout();
            // 
            // listDevices
            // 
            this.listDevices.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listDevices.BackColor = System.Drawing.SystemColors.Window;
            this.listDevices.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.listDevices.FullRowSelect = true;
            this.listDevices.HideSelection = false;
            this.listDevices.Location = new System.Drawing.Point(2, 17);
            this.listDevices.Margin = new System.Windows.Forms.Padding(2);
            this.listDevices.MultiSelect = false;
            this.listDevices.Name = "listDevices";
            this.listDevices.Size = new System.Drawing.Size(194, 300);
            this.listDevices.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.listDevices.TabIndex = 12;
            this.listDevices.TileSize = new System.Drawing.Size(238, 40);
            this.listDevices.UseCompatibleStateImageBehavior = false;
            this.listDevices.View = System.Windows.Forms.View.Tile;
            this.listDevices.SelectedIndexChanged += new System.EventHandler(this.listDevices_SelectedIndexChanged_1);
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(2, 2);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(133, 20);
            this.label4.TabIndex = 13;
            this.label4.Text = "Connected Devices";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.checkEnableAsCommunicationsOnStartup);
            this.groupBox1.Controls.Add(this.checkEnableAsMultimediaOnStartup);
            this.groupBox1.Controls.Add(this.checkCustomName);
            this.groupBox1.Controls.Add(this.pictureModded);
            this.groupBox1.Controls.Add(this.textCustomName);
            this.groupBox1.Controls.Add(this.label12);
            this.groupBox1.Controls.Add(this.checkHideDevice);
            this.groupBox1.Controls.Add(this.label13);
            this.groupBox1.Controls.Add(this.label14);
            this.groupBox1.Controls.Add(this.label15);
            this.groupBox1.Controls.Add(this.buttonRemove);
            this.groupBox1.Controls.Add(this.buttonSave);
            this.groupBox1.Controls.Add(this.trackBrightness);
            this.groupBox1.Controls.Add(this.trackSaturation);
            this.groupBox1.Controls.Add(this.trackHue);
            this.groupBox1.Location = new System.Drawing.Point(200, 2);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox1.Size = new System.Drawing.Size(321, 279);
            this.groupBox1.TabIndex = 14;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Selected Device Settings";
            // 
            // checkEnableAsCommunicationsOnStartup
            // 
            this.checkEnableAsCommunicationsOnStartup.Location = new System.Drawing.Point(14, 184);
            this.checkEnableAsCommunicationsOnStartup.Margin = new System.Windows.Forms.Padding(2);
            this.checkEnableAsCommunicationsOnStartup.Name = "checkEnableAsCommunicationsOnStartup";
            this.checkEnableAsCommunicationsOnStartup.Size = new System.Drawing.Size(285, 16);
            this.checkEnableAsCommunicationsOnStartup.TabIndex = 28;
            this.checkEnableAsCommunicationsOnStartup.Text = "Enable as communications device on startup";
            this.checkEnableAsCommunicationsOnStartup.UseVisualStyleBackColor = true;
            // 
            // checkEnableAsMultimediaOnStartup
            // 
            this.checkEnableAsMultimediaOnStartup.Location = new System.Drawing.Point(14, 164);
            this.checkEnableAsMultimediaOnStartup.Margin = new System.Windows.Forms.Padding(2);
            this.checkEnableAsMultimediaOnStartup.Name = "checkEnableAsMultimediaOnStartup";
            this.checkEnableAsMultimediaOnStartup.Size = new System.Drawing.Size(263, 16);
            this.checkEnableAsMultimediaOnStartup.TabIndex = 27;
            this.checkEnableAsMultimediaOnStartup.Text = "Enable as multimedia device on startup";
            this.checkEnableAsMultimediaOnStartup.UseVisualStyleBackColor = true;
            // 
            // checkCustomName
            // 
            this.checkCustomName.Location = new System.Drawing.Point(14, 109);
            this.checkCustomName.Margin = new System.Windows.Forms.Padding(2);
            this.checkCustomName.Name = "checkCustomName";
            this.checkCustomName.Size = new System.Drawing.Size(149, 18);
            this.checkCustomName.TabIndex = 3;
            this.checkCustomName.Text = "Customize display name";
            this.checkCustomName.UseVisualStyleBackColor = true;
            this.checkCustomName.CheckedChanged += new System.EventHandler(this.checkCustomName_CheckedChanged_1);
            // 
            // pictureModded
            // 
            this.pictureModded.Location = new System.Drawing.Point(288, 15);
            this.pictureModded.Margin = new System.Windows.Forms.Padding(2);
            this.pictureModded.Name = "pictureModded";
            this.pictureModded.Size = new System.Drawing.Size(24, 24);
            this.pictureModded.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureModded.TabIndex = 20;
            this.pictureModded.TabStop = false;
            // 
            // textCustomName
            // 
            this.textCustomName.Enabled = false;
            this.textCustomName.Location = new System.Drawing.Point(14, 131);
            this.textCustomName.Margin = new System.Windows.Forms.Padding(2);
            this.textCustomName.Name = "textCustomName";
            this.textCustomName.Size = new System.Drawing.Size(190, 20);
            this.textCustomName.TabIndex = 4;
            // 
            // label12
            // 
            this.label12.Location = new System.Drawing.Point(233, 19);
            this.label12.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(66, 17);
            this.label12.TabIndex = 26;
            this.label12.Text = "Tray Icon Color";
            // 
            // checkHideDevice
            // 
            this.checkHideDevice.Location = new System.Drawing.Point(4, 255);
            this.checkHideDevice.Margin = new System.Windows.Forms.Padding(2);
            this.checkHideDevice.Name = "checkHideDevice";
            this.checkHideDevice.Size = new System.Drawing.Size(177, 16);
            this.checkHideDevice.TabIndex = 5;
            this.checkHideDevice.Text = "Hide device from switch list";
            this.checkHideDevice.UseVisualStyleBackColor = true;
            // 
            // label13
            // 
            this.label13.Location = new System.Drawing.Point(50, 30);
            this.label13.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(27, 20);
            this.label13.TabIndex = 16;
            this.label13.Text = "Hue";
            // 
            // label14
            // 
            this.label14.Location = new System.Drawing.Point(21, 50);
            this.label14.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(56, 18);
            this.label14.TabIndex = 19;
            this.label14.Text = "Saturation";
            // 
            // label15
            // 
            this.label15.Location = new System.Drawing.Point(20, 71);
            this.label15.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(57, 14);
            this.label15.TabIndex = 21;
            this.label15.Text = "Brightness";
            // 
            // buttonRemove
            // 
            this.buttonRemove.Location = new System.Drawing.Point(108, 207);
            this.buttonRemove.Margin = new System.Windows.Forms.Padding(2);
            this.buttonRemove.Name = "buttonRemove";
            this.buttonRemove.Size = new System.Drawing.Size(100, 33);
            this.buttonRemove.TabIndex = 6;
            this.buttonRemove.Text = "Remove Settings";
            this.buttonRemove.UseVisualStyleBackColor = true;
            this.buttonRemove.Click += new System.EventHandler(this.buttonRemove_Click);
            // 
            // buttonSave
            // 
            this.buttonSave.Location = new System.Drawing.Point(212, 207);
            this.buttonSave.Margin = new System.Windows.Forms.Padding(2);
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(100, 33);
            this.buttonSave.TabIndex = 7;
            this.buttonSave.Text = "Save Settings";
            this.buttonSave.UseVisualStyleBackColor = true;
            this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
            // 
            // trackBrightness
            // 
            this.trackBrightness.BackColor = System.Drawing.SystemColors.Window;
            this.trackBrightness.Location = new System.Drawing.Point(70, 70);
            this.trackBrightness.Margin = new System.Windows.Forms.Padding(2);
            this.trackBrightness.Maximum = 60;
            this.trackBrightness.Name = "trackBrightness";
            this.trackBrightness.Size = new System.Drawing.Size(131, 45);
            this.trackBrightness.TabIndex = 2;
            this.trackBrightness.TickStyle = System.Windows.Forms.TickStyle.None;
            this.trackBrightness.Scroll += new System.EventHandler(this.trackBrightness_Scroll);
            // 
            // trackSaturation
            // 
            this.trackSaturation.BackColor = System.Drawing.SystemColors.Window;
            this.trackSaturation.Location = new System.Drawing.Point(70, 49);
            this.trackSaturation.Margin = new System.Windows.Forms.Padding(2);
            this.trackSaturation.Maximum = 100;
            this.trackSaturation.Name = "trackSaturation";
            this.trackSaturation.Size = new System.Drawing.Size(131, 45);
            this.trackSaturation.TabIndex = 1;
            this.trackSaturation.TickStyle = System.Windows.Forms.TickStyle.None;
            this.trackSaturation.Scroll += new System.EventHandler(this.trackSaturation_Scroll);
            // 
            // trackHue
            // 
            this.trackHue.BackColor = System.Drawing.SystemColors.Window;
            this.trackHue.Location = new System.Drawing.Point(70, 29);
            this.trackHue.Margin = new System.Windows.Forms.Padding(2);
            this.trackHue.Maximum = 360;
            this.trackHue.Name = "trackHue";
            this.trackHue.Size = new System.Drawing.Size(131, 45);
            this.trackHue.TabIndex = 22;
            this.trackHue.TickStyle = System.Windows.Forms.TickStyle.None;
            this.trackHue.Scroll += new System.EventHandler(this.trackHue_Scroll);
            // 
            // Devices
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.listDevices);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.groupBox1);
            this.Name = "Devices";
            this.Size = new System.Drawing.Size(528, 319);
            this.Load += new System.EventHandler(this.Devices_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureModded)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBrightness)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackSaturation)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackHue)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Controls.CustomListView listDevices;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox checkEnableAsCommunicationsOnStartup;
        private System.Windows.Forms.CheckBox checkEnableAsMultimediaOnStartup;
        private System.Windows.Forms.CheckBox checkCustomName;
        private System.Windows.Forms.PictureBox pictureModded;
        private System.Windows.Forms.TextBox textCustomName;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.CheckBox checkHideDevice;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Button buttonRemove;
        private System.Windows.Forms.Button buttonSave;
        private System.Windows.Forms.TrackBar trackBrightness;
        private System.Windows.Forms.TrackBar trackSaturation;
        private System.Windows.Forms.TrackBar trackHue;
    }
}
