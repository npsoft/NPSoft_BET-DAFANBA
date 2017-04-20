using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Awesomium.Core;
using Awesomium.Windows.Forms;
using SpiralEdge.Helper;
using SpiralEdge.Model;

namespace SpiralEdge
{
    public static class Program
    {
        public static ConfigModel Config = null;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                /* -: AllocConsole();*/
                /* -: Config = new ConfigModel(true);
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new frmMain());*/
                /* -: AGIN_3840x2160_Baccarat output = null;
                string file_path = @"D:\NPSoft_BET-DAFANBA\doc\170410\agin-170412-021437-607.png";
                ImageHelper.AnalysisImg_AGIN_3840x2160(file_path, out output);*/
                /* -: Config = new ConfigModel(true);
                Config.Analysis1_AGIN();*/
                /* -: Config = new ConfigModel(true);
                Config.Analysis2_AGIN();*/
                Config = new ConfigModel(true);
            }
            catch (Exception ex)
            {
                if (null != Config && null != Config.Log)
                {
                    Config.Log.Log(ex);
                }
                ConfigModel.SendEmailEx(Config, ex);
                System.Diagnostics.Debug.Print(string.Format("Exception\t:: {0}{1}", ex.Message, ex.StackTrace));
            }
            finally
            {
                if (null != Config && null != Config.Log)
                {
                    Config.Log.Log(string.Format("Information\t:: ------------------ APPLICATION CLOSING -----------------------"));
                }
                if (null != Config && null != Config.ConnHelper)
                {
                    Config.ConnHelper.Close();
                }
            }
        }
        
        internal static void PrintCtrl(Control ctrl, string name)
        {
            Graphics g = ctrl.CreateGraphics();
            if (0 != ctrl.Width || 0 != ctrl.Height)
            {
                string path = Path.Combine(Config.CONFIG_DAFANBA_DIR_PRINT, name);

                Bitmap bmp = new Bitmap(ctrl.Width, ctrl.Height);
                ctrl.DrawToBitmap(bmp, new Rectangle(0, 0, ctrl.Width, ctrl.Height));
                bmp.Save(path, ImageFormat.Jpeg);
                bmp.Dispose();

                Config.Log.Log(string.Format("Information\t:: Printed | {0}", path));
            }
        }

        internal static void OnShowCreatedWebView(object sender, ShowCreatedWebViewEventArgs e)
        {
            IWebView view = sender as IWebView;
            if (null == view) { return; }
            if (!view.IsLive) { return; }

            frmMain mainForm = Application.OpenForms.OfType<frmMain>().FirstOrDefault();
            if (null == mainForm) { return; }

            if (e.IsPopup && !e.IsUserSpecsOnly)
            {
                Rectangle screenRect = e.Specs.InitialPosition.ToRectangle();
                frmChild newWindow = new frmChild(e.NewViewInstance)
                {
                    ShowInTaskbar = true,
                    ClientSize = screenRect.Size != Size.Empty ? screenRect.Size : new Size(640, 480)
                };
                if (0 < screenRect.Width && 0 < screenRect.Height)
                {
                    newWindow.Width = screenRect.Width;
                    newWindow.Height = screenRect.Height;
                }
                newWindow.Show();
                if (Point.Empty != screenRect.Location)
                {
                    newWindow.DesktopLocation = screenRect.Location;
                }
            }
            else if (e.IsWindowOpen || e.IsPost)
            {
                frmChild doc = new frmChild(e.NewViewInstance);
            }
            else
            {
                e.Cancel = true;
                frmChild doc = new frmChild(e.TargetURL);
            }
        }

        [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]
        [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
        static extern bool AllocConsole();
    }
}
