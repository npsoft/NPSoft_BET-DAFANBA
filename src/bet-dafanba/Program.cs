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
                Config = new ConfigModel(true);
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new frmMain());
                /* -: GIN_3840x2160_Baccarat output = null;
                string file_path = @"H:\bet-dafanba\doc\img-02\agin-170318-001916-110.png";
                ImageHelper.AnalysisImg_AGIN_3840x2160(file_path, out output);*/
            }
            catch (Exception ex)
            {
                if (null != Config && null != Config.Log)
                {
                    Config.Log.Log(ex);
                }
                ConfigModel.SendEmailEx(Config, ex);
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
    }
}
