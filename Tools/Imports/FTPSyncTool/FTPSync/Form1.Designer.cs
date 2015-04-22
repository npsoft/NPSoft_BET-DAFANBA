namespace ABSoft.Photobookmart.FTPSync
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
            this.lblFTPHost = new System.Windows.Forms.Label();
            this.txtFTPHost = new System.Windows.Forms.TextBox();
            this.txtUserName = new System.Windows.Forms.TextBox();
            this.lblUserName = new System.Windows.Forms.Label();
            this.lblFTPDefaultPath = new System.Windows.Forms.Label();
            this.lblPassword = new System.Windows.Forms.Label();
            this.lblPort = new System.Windows.Forms.Label();
            this.lblIsSSL = new System.Windows.Forms.Label();
            this.lblLocalPath = new System.Windows.Forms.Label();
            this.lblSyncsTime = new System.Windows.Forms.Label();
            this.txtFTPDefaultPath = new System.Windows.Forms.TextBox();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.txtPort = new System.Windows.Forms.TextBox();
            this.txtLocalPath = new System.Windows.Forms.TextBox();
            this.txtSyncsTime = new System.Windows.Forms.TextBox();
            this.btnFTPConfigSave = new System.Windows.Forms.Button();
            this.bServiceInstall = new System.Windows.Forms.Button();
            this.bRemoveService = new System.Windows.Forms.Button();
            this.bStopService = new System.Windows.Forms.Button();
            this.bStartService = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.cConnectionmode = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cDeleteAfterSync = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.nExpirationMonths = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.cSyncCompleted = new System.Windows.Forms.CheckBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.cSSL = new System.Windows.Forms.ComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.nExpirationMonths)).BeginInit();
            this.SuspendLayout();
            // 
            // lblFTPHost
            // 
            this.lblFTPHost.AutoSize = true;
            this.lblFTPHost.Location = new System.Drawing.Point(13, 12);
            this.lblFTPHost.Name = "lblFTPHost";
            this.lblFTPHost.Size = new System.Drawing.Size(64, 13);
            this.lblFTPHost.TabIndex = 0;
            this.lblFTPHost.Text = "1. FTP Host";
            // 
            // txtFTPHost
            // 
            this.txtFTPHost.Location = new System.Drawing.Point(127, 9);
            this.txtFTPHost.Multiline = true;
            this.txtFTPHost.Name = "txtFTPHost";
            this.txtFTPHost.Size = new System.Drawing.Size(250, 20);
            this.txtFTPHost.TabIndex = 1;
            this.txtFTPHost.Text = "192.168.1.5";
            // 
            // txtUserName
            // 
            this.txtUserName.Location = new System.Drawing.Point(127, 37);
            this.txtUserName.Multiline = true;
            this.txtUserName.Name = "txtUserName";
            this.txtUserName.Size = new System.Drawing.Size(250, 20);
            this.txtUserName.TabIndex = 3;
            this.txtUserName.Text = "chinh1";
            // 
            // lblUserName
            // 
            this.lblUserName.AutoSize = true;
            this.lblUserName.Location = new System.Drawing.Point(13, 40);
            this.lblUserName.Name = "lblUserName";
            this.lblUserName.Size = new System.Drawing.Size(72, 13);
            this.lblUserName.TabIndex = 2;
            this.lblUserName.Text = "2. User Name";
            // 
            // lblFTPDefaultPath
            // 
            this.lblFTPDefaultPath.AutoSize = true;
            this.lblFTPDefaultPath.Location = new System.Drawing.Point(13, 96);
            this.lblFTPDefaultPath.Name = "lblFTPDefaultPath";
            this.lblFTPDefaultPath.Size = new System.Drawing.Size(101, 13);
            this.lblFTPDefaultPath.TabIndex = 10;
            this.lblFTPDefaultPath.Text = "4. FTP Default Path";
            // 
            // lblPassword
            // 
            this.lblPassword.AutoSize = true;
            this.lblPassword.Location = new System.Drawing.Point(13, 68);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.Size = new System.Drawing.Size(65, 13);
            this.lblPassword.TabIndex = 9;
            this.lblPassword.Text = "3. Password";
            // 
            // lblPort
            // 
            this.lblPort.AutoSize = true;
            this.lblPort.Location = new System.Drawing.Point(13, 150);
            this.lblPort.Name = "lblPort";
            this.lblPort.Size = new System.Drawing.Size(38, 13);
            this.lblPort.TabIndex = 12;
            this.lblPort.Text = "6. Port";
            // 
            // lblIsSSL
            // 
            this.lblIsSSL.AutoSize = true;
            this.lblIsSSL.Location = new System.Drawing.Point(12, 122);
            this.lblIsSSL.Name = "lblIsSSL";
            this.lblIsSSL.Size = new System.Drawing.Size(50, 13);
            this.lblIsSSL.TabIndex = 11;
            this.lblIsSSL.Text = "5. Is SSL";
            // 
            // lblLocalPath
            // 
            this.lblLocalPath.AutoSize = true;
            this.lblLocalPath.Location = new System.Drawing.Point(12, 203);
            this.lblLocalPath.Name = "lblLocalPath";
            this.lblLocalPath.Size = new System.Drawing.Size(70, 13);
            this.lblLocalPath.TabIndex = 14;
            this.lblLocalPath.Text = "8. Local Path";
            // 
            // lblSyncsTime
            // 
            this.lblSyncsTime.AutoSize = true;
            this.lblSyncsTime.Location = new System.Drawing.Point(13, 175);
            this.lblSyncsTime.Name = "lblSyncsTime";
            this.lblSyncsTime.Size = new System.Drawing.Size(74, 13);
            this.lblSyncsTime.TabIndex = 13;
            this.lblSyncsTime.Text = "7. Syncs Time";
            // 
            // txtFTPDefaultPath
            // 
            this.txtFTPDefaultPath.Location = new System.Drawing.Point(127, 93);
            this.txtFTPDefaultPath.Multiline = true;
            this.txtFTPDefaultPath.Name = "txtFTPDefaultPath";
            this.txtFTPDefaultPath.Size = new System.Drawing.Size(250, 20);
            this.txtFTPDefaultPath.TabIndex = 18;
            this.txtFTPDefaultPath.Text = "/";
            // 
            // txtPassword
            // 
            this.txtPassword.Location = new System.Drawing.Point(127, 65);
            this.txtPassword.Multiline = true;
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.Size = new System.Drawing.Size(250, 20);
            this.txtPassword.TabIndex = 17;
            this.txtPassword.Text = "12345";
            // 
            // txtPort
            // 
            this.txtPort.Location = new System.Drawing.Point(127, 147);
            this.txtPort.Multiline = true;
            this.txtPort.Name = "txtPort";
            this.txtPort.Size = new System.Drawing.Size(250, 20);
            this.txtPort.TabIndex = 20;
            this.txtPort.Text = "21";
            // 
            // txtLocalPath
            // 
            this.txtLocalPath.Location = new System.Drawing.Point(127, 200);
            this.txtLocalPath.Multiline = true;
            this.txtLocalPath.Name = "txtLocalPath";
            this.txtLocalPath.Size = new System.Drawing.Size(250, 20);
            this.txtLocalPath.TabIndex = 22;
            this.txtLocalPath.Text = "J:/";
            // 
            // txtSyncsTime
            // 
            this.txtSyncsTime.Location = new System.Drawing.Point(127, 172);
            this.txtSyncsTime.Multiline = true;
            this.txtSyncsTime.Name = "txtSyncsTime";
            this.txtSyncsTime.Size = new System.Drawing.Size(250, 20);
            this.txtSyncsTime.TabIndex = 21;
            this.txtSyncsTime.Text = "6";
            // 
            // btnFTPConfigSave
            // 
            this.btnFTPConfigSave.Location = new System.Drawing.Point(3, 363);
            this.btnFTPConfigSave.Name = "btnFTPConfigSave";
            this.btnFTPConfigSave.Size = new System.Drawing.Size(82, 23);
            this.btnFTPConfigSave.TabIndex = 25;
            this.btnFTPConfigSave.Text = "Save Config";
            this.btnFTPConfigSave.UseVisualStyleBackColor = true;
            this.btnFTPConfigSave.Click += new System.EventHandler(this.btnFTPConfigSave_Click);
            // 
            // bServiceInstall
            // 
            this.bServiceInstall.Location = new System.Drawing.Point(3, 392);
            this.bServiceInstall.Name = "bServiceInstall";
            this.bServiceInstall.Size = new System.Drawing.Size(144, 23);
            this.bServiceInstall.TabIndex = 26;
            this.bServiceInstall.Text = "Install Service and Start";
            this.bServiceInstall.UseVisualStyleBackColor = true;
            this.bServiceInstall.Click += new System.EventHandler(this.button1_Click_1);
            // 
            // bRemoveService
            // 
            this.bRemoveService.Location = new System.Drawing.Point(153, 392);
            this.bRemoveService.Name = "bRemoveService";
            this.bRemoveService.Size = new System.Drawing.Size(103, 23);
            this.bRemoveService.TabIndex = 27;
            this.bRemoveService.Text = "Remove Service";
            this.bRemoveService.UseVisualStyleBackColor = true;
            this.bRemoveService.Click += new System.EventHandler(this.bRemoveService_Click);
            // 
            // bStopService
            // 
            this.bStopService.Location = new System.Drawing.Point(114, 421);
            this.bStopService.Name = "bStopService";
            this.bStopService.Size = new System.Drawing.Size(103, 23);
            this.bStopService.TabIndex = 28;
            this.bStopService.Text = "Stop Service";
            this.bStopService.UseVisualStyleBackColor = true;
            this.bStopService.Click += new System.EventHandler(this.bStopService_Click);
            // 
            // bStartService
            // 
            this.bStartService.Location = new System.Drawing.Point(3, 421);
            this.bStartService.Name = "bStartService";
            this.bStartService.Size = new System.Drawing.Size(103, 23);
            this.bStartService.TabIndex = 29;
            this.bStartService.Text = "Start Service";
            this.bStartService.UseVisualStyleBackColor = true;
            this.bStartService.Click += new System.EventHandler(this.bStartService_Click);
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(223, 421);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(82, 23);
            this.button5.TabIndex = 30;
            this.button5.Text = "About";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 229);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(103, 13);
            this.label1.TabIndex = 31;
            this.label1.Text = "9. Connection Mode";
            // 
            // cConnectionmode
            // 
            this.cConnectionmode.FormattingEnabled = true;
            this.cConnectionmode.Items.AddRange(new object[] {
            "Auto",
            "Active",
            "Passive"});
            this.cConnectionmode.Location = new System.Drawing.Point(127, 226);
            this.cConnectionmode.Name = "cConnectionmode";
            this.cConnectionmode.Size = new System.Drawing.Size(250, 21);
            this.cConnectionmode.TabIndex = 32;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 256);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(108, 13);
            this.label2.TabIndex = 33;
            this.label2.Text = "10. Delete After Sync";
            // 
            // cDeleteAfterSync
            // 
            this.cDeleteAfterSync.AutoSize = true;
            this.cDeleteAfterSync.Location = new System.Drawing.Point(127, 256);
            this.cDeleteAfterSync.Name = "cDeleteAfterSync";
            this.cDeleteAfterSync.Size = new System.Drawing.Size(44, 17);
            this.cDeleteAfterSync.TabIndex = 34;
            this.cDeleteAfterSync.Text = "Yes";
            this.cDeleteAfterSync.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 279);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(125, 13);
            this.label3.TabIndex = 35;
            this.label3.Text = "11. Delete expiration files";
            // 
            // nExpirationMonths
            // 
            this.nExpirationMonths.Location = new System.Drawing.Point(143, 277);
            this.nExpirationMonths.Maximum = new decimal(new int[] {
            52,
            0,
            0,
            0});
            this.nExpirationMonths.Name = "nExpirationMonths";
            this.nExpirationMonths.Size = new System.Drawing.Size(49, 20);
            this.nExpirationMonths.TabIndex = 36;
            this.nExpirationMonths.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.nExpirationMonths.ValueChanged += new System.EventHandler(this.nExpirationMonths_ValueChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(24, 300);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(305, 13);
            this.label4.TabIndex = 37;
            this.label4.Text = "Delete on server if files had been created more than XX months";
            // 
            // cSyncCompleted
            // 
            this.cSyncCompleted.AutoSize = true;
            this.cSyncCompleted.Location = new System.Drawing.Point(128, 322);
            this.cSyncCompleted.Name = "cSyncCompleted";
            this.cSyncCompleted.Size = new System.Drawing.Size(172, 17);
            this.cSyncCompleted.TabIndex = 39;
            this.cSyncCompleted.Text = "Sync completed orders file only";
            this.cSyncCompleted.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(13, 322);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(78, 13);
            this.label5.TabIndex = 38;
            this.label5.Text = "12. Sync mode";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(198, 279);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(41, 13);
            this.label6.TabIndex = 40;
            this.label6.Text = "months";
            // 
            // cSSL
            // 
            this.cSSL.FormattingEnabled = true;
            this.cSSL.Items.AddRange(new object[] {
            "None",
            "Implicit",
            "Explicit"});
            this.cSSL.Location = new System.Drawing.Point(127, 120);
            this.cSSL.Name = "cSSL";
            this.cSSL.Size = new System.Drawing.Size(250, 21);
            this.cSSL.TabIndex = 41;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(383, 449);
            this.Controls.Add(this.cSSL);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.cSyncCompleted);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.nExpirationMonths);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.cDeleteAfterSync);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cConnectionmode);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.bStartService);
            this.Controls.Add(this.bStopService);
            this.Controls.Add(this.bRemoveService);
            this.Controls.Add(this.bServiceInstall);
            this.Controls.Add(this.btnFTPConfigSave);
            this.Controls.Add(this.txtLocalPath);
            this.Controls.Add(this.txtSyncsTime);
            this.Controls.Add(this.txtPort);
            this.Controls.Add(this.txtFTPDefaultPath);
            this.Controls.Add(this.txtPassword);
            this.Controls.Add(this.lblLocalPath);
            this.Controls.Add(this.lblSyncsTime);
            this.Controls.Add(this.lblPort);
            this.Controls.Add(this.lblIsSSL);
            this.Controls.Add(this.lblFTPDefaultPath);
            this.Controls.Add(this.lblPassword);
            this.Controls.Add(this.txtUserName);
            this.Controls.Add(this.lblUserName);
            this.Controls.Add(this.txtFTPHost);
            this.Controls.Add(this.lblFTPHost);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form1";
            this.Text = "Photobookmart FTP Sync";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.nExpirationMonths)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblFTPHost;
        private System.Windows.Forms.TextBox txtFTPHost;
        private System.Windows.Forms.TextBox txtUserName;
        private System.Windows.Forms.Label lblUserName;
        private System.Windows.Forms.Label lblFTPDefaultPath;
        private System.Windows.Forms.Label lblPassword;
        private System.Windows.Forms.Label lblPort;
        private System.Windows.Forms.Label lblIsSSL;
        private System.Windows.Forms.Label lblLocalPath;
        private System.Windows.Forms.Label lblSyncsTime;
        private System.Windows.Forms.TextBox txtFTPDefaultPath;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.TextBox txtPort;
        private System.Windows.Forms.TextBox txtLocalPath;
        private System.Windows.Forms.TextBox txtSyncsTime;
        private System.Windows.Forms.Button btnFTPConfigSave;
        private System.Windows.Forms.Button bServiceInstall;
        private System.Windows.Forms.Button bRemoveService;
        private System.Windows.Forms.Button bStopService;
        private System.Windows.Forms.Button bStartService;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cConnectionmode;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox cDeleteAfterSync;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown nExpirationMonths;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox cSyncCompleted;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox cSSL;
    }
}

