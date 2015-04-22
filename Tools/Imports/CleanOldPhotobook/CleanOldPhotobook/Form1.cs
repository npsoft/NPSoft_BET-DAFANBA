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
using System.Net;
using ABSoft.Photobookmart.CleanOldPhotobook.Helper;

namespace ABSoft.Photobookmart.CleanOldPhotobook
{
    public partial class Form1 : Form
    {
        void UpdateInfo(string st)
        {
            Application.DoEvents();
        }

        #region Events

        public Form1()
        {
            InitializeComponent();
        }

        private void btnFTPConfigSave_Click(object sender, EventArgs e)
        {
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
                ServiceInstaller.InstallAndStart(AppHost.ServiceName, AppHost.ServiceDescription, Application.ExecutablePath);
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
            MessageBox.Show("Version 1.0, build on Oct 13, 2014 by Immanuel");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
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