namespace EMGU.SettingUp
{
    partial class SettingUpFrm
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.label1 = new System.Windows.Forms.Label();
            this.txtBoxFileName = new System.Windows.Forms.TextBox();
            this.btnLoadImg = new System.Windows.Forms.Button();
            this.pictureBoxOrg = new System.Windows.Forms.PictureBox();
            this.pictureBoxNew = new System.Windows.Forms.PictureBox();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.panel1.SuspendLayout();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxOrg)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxNew)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnLoadImg);
            this.panel1.Controls.Add(this.txtBoxFileName);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(784, 53);
            this.panel1.TabIndex = 0;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 53);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.pictureBoxOrg);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.pictureBoxNew);
            this.splitContainer1.Size = new System.Drawing.Size(784, 508);
            this.splitContainer1.SplitterDistance = 392;
            this.splitContainer1.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(26, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "File:";
            // 
            // txtBoxFileName
            // 
            this.txtBoxFileName.Location = new System.Drawing.Point(49, 14);
            this.txtBoxFileName.Name = "txtBoxFileName";
            this.txtBoxFileName.Size = new System.Drawing.Size(215, 20);
            this.txtBoxFileName.TabIndex = 1;
            // 
            // btnLoadImg
            // 
            this.btnLoadImg.Location = new System.Drawing.Point(270, 12);
            this.btnLoadImg.Name = "btnLoadImg";
            this.btnLoadImg.Size = new System.Drawing.Size(30, 23);
            this.btnLoadImg.TabIndex = 2;
            this.btnLoadImg.Text = "...";
            this.btnLoadImg.UseVisualStyleBackColor = true;
            this.btnLoadImg.Click += new System.EventHandler(this.btnLoadImg_Click);
            // 
            // pictureBoxOrg
            // 
            this.pictureBoxOrg.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBoxOrg.Location = new System.Drawing.Point(0, 0);
            this.pictureBoxOrg.Name = "pictureBoxOrg";
            this.pictureBoxOrg.Size = new System.Drawing.Size(392, 508);
            this.pictureBoxOrg.TabIndex = 0;
            this.pictureBoxOrg.TabStop = false;
            // 
            // pictureBoxNew
            // 
            this.pictureBoxNew.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBoxNew.Location = new System.Drawing.Point(0, 0);
            this.pictureBoxNew.Name = "pictureBoxNew";
            this.pictureBoxNew.Size = new System.Drawing.Size(388, 508);
            this.pictureBoxNew.TabIndex = 0;
            this.pictureBoxNew.TabStop = false;
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // SettingUpFrm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 561);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.panel1);
            this.Name = "SettingUpFrm";
            this.Text = "Setting Up";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxOrg)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxNew)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtBoxFileName;
        private System.Windows.Forms.Button btnLoadImg;
        private System.Windows.Forms.PictureBox pictureBoxOrg;
        private System.Windows.Forms.PictureBox pictureBoxNew;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
    }
}