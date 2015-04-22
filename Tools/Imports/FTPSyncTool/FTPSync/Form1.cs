using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using ServiceStack.OrmLite;
using ServiceStack.Common;
using ServiceStack.ServiceInterface.Auth;
using System.Net;
using System.Net.FtpClient;
using ABSoft.Photobookmart.FTPSync.Models;
using ABSoft.Photobookmart.FTPSync.Helper;

namespace ABSoft.Photobookmart.FTPSync
{
    public partial class Form1 : Form
    {
        IDbConnection _connection = null;
        public virtual IDbConnection Db
        {
            get
            {
                if (_connection == null)
                {
                    _connection = AppHost.Resolve<IDbConnection>();

                }

                if (_connection != null && _connection.State != ConnectionState.Open)
                {
                    // force open new connection
                    _connection = AppHost.Resolve<IDbConnectionFactory>().Open();
                }

                return _connection;
            }
        }

        private FTPConfig GetFtpConfig()
        {
            return Db.Select<FTPConfig>(x => x.Where().OrderByDescending(z => z.Id).Limit(1)).FirstOrDefault();
        }

        private void AddFtpConfig()
        {

            int SyncsTime = 24 * 60;

            int.TryParse(txtSyncsTime.Text, out SyncsTime);

            var ftpconfig = Db.Select<FTPConfig>(x => x.Limit(1)).FirstOrDefault();



            ftpconfig.FTPHost = txtFTPHost.Text;
            ftpconfig.UserName = txtUserName.Text;
            ftpconfig.Password = txtPassword.Text;
            ftpconfig.FTPDefaultPath = txtFTPDefaultPath.Text;
            ftpconfig.Port = int.Parse(txtPort.Text);
            ftpconfig.SyncsTime = SyncsTime;
            ftpconfig.LocalPath = txtLocalPath.Text;
            ftpconfig.SSLEncryptionMode = cSSL.SelectedIndex;
            ftpconfig.ConnectionMode = cConnectionmode.SelectedIndex;
            ftpconfig.DeleteAfterSync = cDeleteAfterSync.Checked;
            ftpconfig.SyncOrderOnly = cSyncCompleted.Checked;
            ftpconfig.DeleteExpiredMonths = (int)nExpirationMonths.Value;


            Db.Update<FTPConfig>(ftpconfig);

            Invoke(new Action(() =>
            {
                UpdateInfo(string.Format("Message: {0}", "Add configuration successful."));
            }));
        }

        private void LoadFtpConfig()
        {
            FTPConfig ftpConfig = GetFtpConfig();

            bool flag = (ftpConfig != null) ? true : false;

            if (!flag)
            {
                ftpConfig = new FTPConfig()
                {
                    FTPHost = "127.0.0.1",
                    UserName = "username",
                    Password = "password",
                    FTPDefaultPath = @"/",
                    SyncsTime = 24 * 60,
                    LocalPath = @"C:\",
                    ServerTimeZone = 7,
                    LocalTimeZone = 7,
                    ConnectionMode = 0,
                    DeleteAfterSync = false,
                    SyncOrderOnly = true,
                    DeleteExpiredMonths = 4,
                    SSLEncryptionMode = 0
                };


            }

            txtFTPHost.Text = ftpConfig.FTPHost;
            txtUserName.Text = ftpConfig.UserName;
            txtPassword.Text = ftpConfig.Password;
            txtFTPDefaultPath.Text = ftpConfig.FTPDefaultPath;
            txtPort.Text = ftpConfig.Port.ToString();
            txtSyncsTime.Text = string.Format("{0}", ftpConfig.SyncsTime);
            txtLocalPath.Text = ftpConfig.LocalPath;
            // ssl
            cSSL.SelectedIndex = ftpConfig.SSLEncryptionMode;
            cConnectionmode.SelectedIndex = ftpConfig.ConnectionMode;
            cDeleteAfterSync.Checked = ftpConfig.DeleteAfterSync;
            cSyncCompleted.Checked = ftpConfig.SyncOrderOnly;
            nExpirationMonths.Value = ftpConfig.DeleteExpiredMonths;

            string message = flag ? string.Format("{0}", "Load configuration successful.") : string.Format("{0}", "Load configuration failure. Default configuration loaded.");

            UpdateInfo(string.Format("Message: {0}", message));

            Application.DoEvents();
        }


        void UpdateInfo(string st)
        {
            Application.DoEvents();
        }



        #region Events

        public Form1()
        {
            InitializeComponent();

            LoadFtpConfig();
        }


        private void btnFTPConfigSave_Click(object sender, EventArgs e)
        {
            AddFtpConfig();
            AppHost.Log.Log("Config saving success");
        }

        void UpdateButtonState()
        {
            if (ServiceInstaller.ServiceIsInstalled(AppHost.ServiceName))
            {
                bServiceInstall.Enabled = false;
                bRemoveService.Enabled = true;
            }
            else
            {
                bServiceInstall.Enabled = true;
                bRemoveService.Enabled = false;
            }

            var status = ServiceInstaller.GetServiceStatus(AppHost.ServiceName);
            if (status == ServiceState.Run)
            {
                bStartService.Enabled = false;
                bStopService.Enabled = true;
            }
            else
            {
                bStartService.Enabled = true;
                bStopService.Enabled = false;
            }
        }
        #endregion

        private void button1_Click_1(object sender, EventArgs e)
        {
            try
            {
                ServiceInstaller.InstallAndStart(AppHost.ServiceName, AppHost.ServiceName, Application.ExecutablePath);
                ServiceInstaller.SetRecoveryOptions(AppHost.ServiceName);
                UpdateButtonState();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Version 1.0, build on Oct 4, 2014 by Immanuel");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            cConnectionmode.SelectedIndex = 0;
            cSSL.SelectedIndex = 0;
            LoadFtpConfig();
            UpdateButtonState();

        }

        private void nExpirationMonths_ValueChanged(object sender, EventArgs e)
        {

        }

        private void bRemoveService_Click(object sender, EventArgs e)
        {
            try
            {
                ServiceInstaller.Uninstall(AppHost.ServiceName);
                UpdateButtonState();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void bStopService_Click(object sender, EventArgs e)
        {
            try
            {
                ServiceInstaller.StopService(AppHost.ServiceName);
                UpdateButtonState();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void bStartService_Click(object sender, EventArgs e)
        {
            try
            {
                ServiceInstaller.StartService(AppHost.ServiceName);
                UpdateButtonState();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}