using System.Drawing;

namespace AudioSwitch.Controls
{
    internal partial class VolumeBar
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(VolumeBar));
            this.lblGraph = new System.Windows.Forms.Label();
            this.Thumb = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.Thumb)).BeginInit();
            this.SuspendLayout();
            // 
            // lblGraph
            // 
            this.lblGraph.Font = new System.Drawing.Font("Microsoft Sans Serif", 3.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(186)));
            this.lblGraph.ForeColor = System.Drawing.Color.Gray;
            this.lblGraph.Location = new System.Drawing.Point(-1, 1);
            this.lblGraph.Margin = new System.Windows.Forms.Padding(0);
            this.lblGraph.Name = "lblGraph";
            this.lblGraph.Size = new System.Drawing.Size(196, 7);
            this.lblGraph.TabIndex = 11;
            this.lblGraph.Text = resources.GetString("lblGraph.Text");
            this.lblGraph.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblGraph.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lblGraph_MouseDown);
            this.lblGraph.MouseMove += new System.Windows.Forms.MouseEventHandler(this.lblGraph_MouseMove);
            this.lblGraph.MouseUp += new System.Windows.Forms.MouseEventHandler(this.lblGraph_MouseUp);
            // 
            // Thumb
            // 
            this.Thumb.BackColor = System.Drawing.Color.Transparent;
            this.Thumb.BackgroundImage = global::AudioSwitch.Properties.Resources.ThumbNormal;
            this.Thumb.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.Thumb.Location = new System.Drawing.Point(0, 0);
            this.Thumb.Name = "Thumb";
            this.Thumb.Size = new System.Drawing.Size(31, 9);
            this.Thumb.TabIndex = 13;
            this.Thumb.TabStop = false;
            this.Thumb.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Thumb_MouseDown);
            this.Thumb.MouseEnter += new System.EventHandler(this.Thumb_MouseEnter);
            this.Thumb.MouseLeave += new System.EventHandler(this.Thumb_MouseLeave);
            this.Thumb.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Thumb_MouseMove);
            this.Thumb.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Thumb_MouseUp);
            this.Thumb.Move += new System.EventHandler(this.Thumb_Move);
            // 
            // VolumeBar
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.Thumb);
            this.Controls.Add(this.lblGraph);
            this.Name = "VolumeBar";
            this.Size = new System.Drawing.Size(195, 9);
            ((System.ComponentModel.ISupportInitialize)(this.Thumb)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lblGraph;
        private System.Windows.Forms.PictureBox Thumb;
    }
}
