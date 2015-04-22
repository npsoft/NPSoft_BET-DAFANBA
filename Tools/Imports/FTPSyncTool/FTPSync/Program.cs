using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Win32;
using ABSoft.Photobookmart.FTPSync.ServiceInterface;
using ABSoft.Photobookmart.FTPSync.Helper;
using System.ServiceProcess;

namespace ABSoft.Photobookmart.FTPSync
{
    class Program
    {

        private static void RegisterInStartup(bool isChecked)
        {
            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            if (isChecked)
            {
                registryKey.SetValue("SyncFileServer", Application.ExecutablePath);
            }
            else
            {
                registryKey.DeleteValue("SyncFileServer");
            }
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // handle the exceptions 
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            // Start the local services server and init the log system also
            if (!AppHost.Start())
            {
                AppHost.Log.Dispose();
                return;
            }

            // start the service
            AppHost.FTPService = new FTPSyncService(AppHost.Log);
            if (Environment.UserInteractive || args.Contains("-winform"))
            {
                // load the main form
                //AppHost.FTPService.Start();
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form1());
                //AppHost.FTPService.Stop();
            }
            else
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                          {
                              new FTPSyncWindowsService(AppHost.FTPService)
                          };
                ServiceBase.Run(ServicesToRun);
            }

            // closing the log
            try
            {
                AppHost.DatabaseConnection.Close();
                AppHost.Log.Dispose();
            }
            catch { }
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                if (e.ExceptionObject is Exception)
                {
                    AppHost.Log.Log((Exception)e.ExceptionObject);
                }
                AppHost.Log.Dispose();
            }
            catch
            {
            }
        }
    }
}