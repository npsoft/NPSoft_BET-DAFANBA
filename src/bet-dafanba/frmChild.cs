using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Awesomium.Core;

namespace SpiralEdge
{
    public partial class frmChild : Form
    {
        #region For: Ctors
        public frmChild()
        {
            InitializeComponent();
            InitializeCtrls();
            InitializeEvents();
        }

        public frmChild(Uri url)
        {
            InitializeComponent();
            InitializeCtrls();
            InitializeEvents();
            wcAwesomium.Source = url;
            bindingSource = new BindingSource() { DataSource = wcAwesomium };
            this.DataBindings.Add(new Binding("Text", bindingSource, "Title", true));
        }

        public frmChild(IntPtr nativeView)
        {
            InitializeComponent();
            InitializeCtrls();
            InitializeEvents();
            wcAwesomium.NativeView = nativeView;
            bindingSource = new BindingSource() { DataSource = wcAwesomium };
            this.DataBindings.Add(new Binding("Text", bindingSource, "Title", true));
        }
        #endregion
        #region For: Events
        private void InitializeCtrls()
        {
            #region For: Form
            this.MinimumSize = new Size(800, 600);
            this.MaximumSize = new Size(800, 600);
            this.Size = new Size(800, 600);
            /* -: this.Location = new Point(0, 0);
            this.AutoScroll = true;*/
            #endregion
            #region For: Panel
            panel1.Dock = DockStyle.None;
            panel1.Anchor = AnchorStyles.None;
            panel1.MinimumSize = new Size(3840, 2160);
            panel1.MaximumSize = new Size(3840, 2160);
            panel1.Size = new Size(3840, 2160);
            /* -: panel1.Location = new Point(0, 0);
            panel1.AutoScroll = true;*/
            #endregion
        }

        private void InitializeEvents()
        {
            ThreadCaptureHdl();
            this.FormClosed += OnLogFormClosed;
            this.FormClosed += OnFormClosed;
            wcAwesomium.AddressChanged += OnLogAddressChanged;
            wcAwesomium.TitleChanged += OnLogTitleChanged;
            wcAwesomium.LoadingFrameComplete += OnLogLoadingFrameComplete;
            wcAwesomium.ShowCreatedWebView += OnLogShowCreatedWebView;
            wcAwesomium.LoadingFrameComplete += OnLoadingFrameComplete;
        }

        private void OnLogFormClosed(object sender, FormClosedEventArgs e)
        {
            Program.Config.Log.Log(string.Format("Information\t:: Child | Form Closed"));
        }

        private void OnFormClosed(object sender, FormClosedEventArgs e)
        {
            threadCapture.Abort();
        }

        private void OnLogAddressChanged(object sender, UrlEventArgs e)
        {
            string url = string.Format("{0}", e.Url);
            Program.Config.Log.Log(string.Format("Information\t:: Child | Address Changed | {0}", url));
        }

        private void OnLogTitleChanged(object sender, TitleChangedEventArgs e)
        {
            /* -: string title = string.Format("{0}", e.Title);
            Program.Config.Log.Log(string.Format("Information\t:: Child | Title Changed | {0}", title));*/
        }

        private void OnLogLoadingFrameComplete(object sender, FrameEventArgs e)
        {
            string url = string.Format("{0}", e.Url);
            Program.Config.Log.Log(string.Format("Information\t:: Child | Loading Frame Complete | {0} | {1} | {2}", url, e.IsMainFrame, wcAwesomium.IsLive));
        }

        private void OnLogShowCreatedWebView(object sender, ShowCreatedWebViewEventArgs e)
        {
            string url = string.Format("{0}", e.TargetURL);
            Program.Config.Log.Log(string.Format("Information\t:: Child | Show Created Web View | {0}", url));
        }

        private void OnLoadingFrameComplete(object sender, FrameEventArgs e)
        {
            if (e.IsMainFrame)
            {
                string url = string.Format("{0}", e.Url);
                Program.PrintCtrl(wcAwesomium, string.Format(@"form-child-{0:yyMMdd-HHmmss-fff}.png", DateTime.Now));
            }
        }
        #endregion
        #region For: Methods
        private void ThreadCaptureHdl()
        {
            ThreadPool.SetMaxThreads(0, 1);
            threadCapture = new Thread(new ThreadStart(ThreadCaptureHdlExec));
            threadCapture.SetApartmentState(ApartmentState.STA);
            threadCapture.Priority = ThreadPriority.Highest;
            threadCapture.Start();
        }

        private void ThreadCaptureHdlExec()
        {
            while (true)
            {
                Thread.Sleep(Program.Config.CONFIG_DAFANBA_INTERVAL_CAPTURE_AG);
                WCAwesomiumCallBackHdl(null);
                Application.DoEvents();
            }
        }

        delegate void WCAwesomiumCallBack(object obj);
        private void WCAwesomiumCallBackHdl(object obj)
        {
            if (this.wcAwesomium.InvokeRequired)
            {
                WCAwesomiumCallBack d = new WCAwesomiumCallBack(WCAwesomiumCallBackHdl);
                this.Invoke(d, new object[] { obj });
            }
            else
            {
                string name = string.Format(@"agin-{0:yyMMdd-HHmmss-fff}.png", DateTime.Now);
                Program.PrintCtrl(wcAwesomium, name);
                Program.Config.HdlAGIN(name);
            }
        }
        #endregion
        #region For: Properties
        private BindingSource bindingSource;
        private Thread threadCapture { get; set; }
        #endregion
        #region For: Utilities & Other
        [System.Runtime.InteropServices.DllImport("KERNEL32.DLL", EntryPoint = "SetProcessWorkingSetSize", SetLastError = true, CallingConvention = System.Runtime.InteropServices.CallingConvention.StdCall)]
        internal static extern bool SetProcessWorkingSetSize(IntPtr pProcess, int dwMinimumWorkingSetSize, int dwMaximumWorkingSetSize);

        [System.Runtime.InteropServices.DllImport("KERNEL32.DLL", EntryPoint = "GetCurrentProcess", SetLastError = true, CallingConvention = System.Runtime.InteropServices.CallingConvention.StdCall)]
        internal static extern IntPtr GetCurrentProcess();
        #endregion
    }
}
