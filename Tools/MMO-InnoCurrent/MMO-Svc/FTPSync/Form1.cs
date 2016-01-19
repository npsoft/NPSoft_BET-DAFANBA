using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using ServiceStack.OrmLite;
using ServiceStack.Common;
using ServiceStack.ServiceInterface.Auth;
using System.Net;
using System.Net.FtpClient;
using mshtml;
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

        private void TabControl_DeleteItem(int index)
        {
            tcInnoCurrent.TabPages.RemoveAt(index);
        }

        private void DocumentCompleted_InnoContext(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            ExtendedWebBrowser wb = sender as ExtendedWebBrowser;
            HtmlElement head = wb.Document.GetElementsByTagName("head")[0];
            HtmlElement script = wb.Document.CreateElement("script");
            IHTMLScriptElement element = (IHTMLScriptElement)script.DomElement;
            element.text = @"
            var setIntervalId = setInterval(function () {
                var dDiv = document.getElementById('hid');
                if (dDiv != null) {
                    clearInterval(setIntervalId);
                    var dA = dDiv.children[0];
                    dA.click();
                }
            }, 1 * 1000);";
            head.AppendChild(script);
        }

        void UpdateInfo(string st)
        {
            Application.DoEvents();
        }

        void UpdateButtonState()
        {
            txtFTPHost.Text = string.Format("[ServiceName:{0}, ServiceIsInstalled:{1}, GetServiceStatus:{2}]",
                AppHost.ServiceName, ServiceInstaller.ServiceIsInstalled(AppHost.ServiceName), ServiceInstaller.GetServiceStatus(AppHost.ServiceName));
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

        #region Events

        public Form1()
        {
            InitializeComponent();
            // LoadFtpConfig();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            cConnectionmode.SelectedIndex = 0;
            cSSL.SelectedIndex = 0;
            // LoadFtpConfig();
            UpdateButtonState();
        }

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
                AppHost.Log.Log(ex);
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
                AppHost.Log.Log(ex);
                MessageBox.Show(ex.Message);
            }
        }

        private void btnFTPConfigSave_Click(object sender, EventArgs e)
        {
            AddFtpConfig();
            AppHost.Log.Log("Config saving success");
        }

        #endregion

        private void button5_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Version 1.0, build on Oct 4, 2014 by Immanuel");
        }

        private void InitializeBrowserEvents(ExtendedWebBrowser SourceBrowser)
        {
            SourceBrowser.NewWindow2 += new EventHandler<NewWindow2EventArgs>(SourceBrowser_NewWindow2);
        }

        void SourceBrowser_NewWindow2(object sender, NewWindow2EventArgs e)
        {
            TabPage NewTabPage = new TabPage()
            {
                Text = "Loading..."
            };

            ExtendedWebBrowser NewTabBrowser = new ExtendedWebBrowser()
            {
                Parent = NewTabPage,
                Dock = DockStyle.Fill,
                Tag = NewTabPage,
                ScriptErrorsSuppressed = true
            };
            NewTabBrowser.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(DocumentCompleted_InnoContext);

            e.PPDisp = NewTabBrowser.Application;
            InitializeBrowserEvents(NewTabBrowser);
  
            tcInnoCurrent.TabPages.Add(NewTabPage);
            tcInnoCurrent.SelectedTab = NewTabPage;
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
    }
}
