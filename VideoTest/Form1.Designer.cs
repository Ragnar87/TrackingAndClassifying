using System.ComponentModel;
namespace VideoTest
{
    partial class SalmonCounter
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
            this.play_pause_BTN = new System.Windows.Forms.Panel();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.show = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.show)).BeginInit();
            this.SuspendLayout();
            // 
            // play_pause_BTN
            // 
            this.play_pause_BTN.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.play_pause_BTN.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.play_pause_BTN.Location = new System.Drawing.Point(966, 592);
            this.play_pause_BTN.Name = "play_pause_BTN";
            this.play_pause_BTN.Size = new System.Drawing.Size(50, 50);
            this.play_pause_BTN.TabIndex = 3;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(13, 24);
            this.pictureBox1.MaximumSize = new System.Drawing.Size(1280, 960);
            this.pictureBox1.MinimumSize = new System.Drawing.Size(640, 480);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(640, 480);
            this.pictureBox1.TabIndex = 4;
            this.pictureBox1.TabStop = false;
            // 
            // show
            // 
            this.show.Location = new System.Drawing.Point(933, 24);
            this.show.MaximumSize = new System.Drawing.Size(1280, 960);
            this.show.MinimumSize = new System.Drawing.Size(640, 480);
            this.show.Name = "show";
            this.show.Size = new System.Drawing.Size(720, 640);
            this.show.TabIndex = 5;
            this.show.TabStop = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1849, 537);
            this.Controls.Add(this.show);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.play_pause_BTN);
            this.Name = "Form1";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.show)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel play_pause_BTN;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.PictureBox show;
    }
}

