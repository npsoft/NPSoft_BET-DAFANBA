using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Win32;
using ABSoft.Photobookmart.CleanOldPhotobook.ServiceInterface;
using ABSoft.Photobookmart.CleanOldPhotobook.Helper;
using System.ServiceProcess;
using ABSoft.Photobookmart.CleanOldPhotobook.Components;

namespace ABSoft.Photobookmart.CleanOldPhotobook
{
    class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // handle the exceptions 
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            // start the service
            AppHost.Log = new RBLog();

            AppHost.Service = new PhotobookmartService(AppHost.Log);
            if (Environment.UserInteractive || args.Contains("-winform"))
            {
                // load the main form
                //AppHost.Service.Start();
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form1());
               AppHost.Service.Stop();
            }
            else
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                          {
                              new FTPSyncWindowsService(AppHost.Service)
                          };
                ServiceBase.Run(ServicesToRun);
            }

            // closing the log
            try
            {
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