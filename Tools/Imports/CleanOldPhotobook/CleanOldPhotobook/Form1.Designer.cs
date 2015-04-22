namespace ABSoft.Photobookmart.CleanOldPhotobook
{
    partial class Form1
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
            this.bServiceInstall = new System.Windows.Forms.Button();
            this.bRemoveService = new System.Windows.Forms.Button();
            this.bStopService = new System.Windows.Forms.Button();
            this.bStartService = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // bServiceInstall
            // 
            this.bServiceInstall.Location = new System.Drawing.Point(12, 12);
            this.bServiceInstall.Name = "bServiceInstall";
            this.bServiceInstall.Size = new System.Drawing.Size(144, 23);
            this.bServiceInstall.TabIndex = 26;
            this.bServiceInstall.Text = "Install Service and Start";
            this.bServiceInstall.UseVisualStyleBackColor = true;
            this.bServiceInstall.Click += new System.EventHandler(this.button1_Click_1);
            // 
            // bRemoveService
            // 
            this.bRemoveService.Location = new System.Drawing.Point(162, 12);
            this.bRemoveService.Name = "bRemoveService";
            this.bRemoveService.Size = new System.Drawing.Size(103, 23);
            this.bRemoveService.TabIndex = 27;
            this.bRemoveService.Text = "Remove Service";
            this.bRemoveService.UseVisualStyleBackColor = true;
            this.bRemoveService.Click += new System.EventHandler(this.bRemoveService_Click);
            // 
            // bStopService
            // 
            this.bStopService.Location = new System.Drawing.Point(123, 41);
            this.bStopService.Name = "bStopService";
            this.bStopService.Size = new System.Drawing.Size(103, 23);
            this.bStopService.TabIndex = 28;
            this.bStopService.Text = "Stop Service";
            this.bStopService.UseVisualStyleBackColor = true;
            this.bStopService.Click += new System.EventHandler(this.bStopService_Click);
            // 
            // bStartService
            // 
            this.bStartService.Location = new System.Drawing.Point(12, 41);
            this.bStartService.Name = "bStartService";
            this.bStartService.Size = new System.Drawing.Size(103, 23);
            this.bStartService.TabIndex = 29;
            this.bStartService.Text = "Start Service";
            this.bStartService.UseVisualStyleBackColor = true;
            this.bStartService.Click += new System.EventHandler(this.bStartService_Click);
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(232, 41);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(82, 23);
            this.button5.TabIndex = 30;
            this.button5.Text = "About";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(383, 81);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.bStartService);
            this.Controls.Add(this.bStopService);
            this.Controls.Add(this.bRemoveService);
            this.Controls.Add(this.bServiceInstall);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form1";
            this.Text = "Photobookmart Clean Old Photobook Service";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button bServiceInstall;
        private System.Windows.Forms.Button bRemoveService;
        private System.Windows.Forms.Button bStopService;
        private System.Windows.Forms.Button bStartService;
        private System.Windows.Forms.Button button5;
    }
}

