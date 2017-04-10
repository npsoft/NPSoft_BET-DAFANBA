using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using Awesomium.Core;
using Awesomium.Windows.Controls;

namespace SpiralEdge
{
    public partial class frmMain : Form
    {
        #region For: Ctors
        public frmMain()
        {
            InitializeComponent();
            InitializeEvents();
            wcAwesomium.Source = new Uri(Program.Config.CONFIG_DAFANBA_URL_DEFAULT);
            bindingSource = new BindingSource() { DataSource = wcAwesomium };
            this.DataBindings.Add(new Binding("Text", bindingSource, "Title", true));
        }
        #endregion
        #region For: Events
        private void InitializeEvents()
        {
            this.FormClosed += OnLogFormClosed;
            this.FormClosed += OnFormClosed;
            wcAwesomium.AddressChanged += OnLogAddressChanged;
            wcAwesomium.TitleChanged += OnLogTitleChanged;
            wcAwesomium.LoadingFrameComplete += OnLogLoadingFrameComplete;
            wcAwesomium.ShowCreatedWebView += OnLogShowCreatedWebView;
            wcAwesomium.LoadingFrameComplete += OnLoadingFrameComplete;
            wcAwesomium.ShowCreatedWebView += Program.OnShowCreatedWebView;
        }

        private void OnLogFormClosed(object sender, FormClosedEventArgs e)
        {
            Program.Config.Log.Log(string.Format("Information\t:: Main | Form Closed"));
        }

        private void OnFormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        private void OnLogAddressChanged(object sender, UrlEventArgs e)
        {
            string url = string.Format("{0}", e.Url);
            Program.Config.Log.Log(string.Format("Information\t:: Main | Address Changed | {0}", url));
        }

        private void OnLogTitleChanged(object sender, TitleChangedEventArgs e)
        {
            /* -: string title = string.Format("{0}", e.Title);
            Program.Config.Log.Log(string.Format("Information\t:: Main | Title Changed | {0}", title));*/
        }

        private void OnLogLoadingFrameComplete(object sender, FrameEventArgs e)
        {
            string url = string.Format("{0}", e.Url);
            Program.Config.Log.Log(string.Format("Information\t:: Main | Loading Frame Complete | {0} | {1} | {2}", url, e.IsMainFrame, wcAwesomium.IsLive));
        }

        private void OnLogShowCreatedWebView(object sender, ShowCreatedWebViewEventArgs e)
        {
            string url = string.Format("{0}", e.TargetURL);
            Program.Config.Log.Log(string.Format("Information\t:: Main | Show Created Web View | {0}", url));
        }

        private void OnLoadingFrameComplete(object sender, FrameEventArgs e)
        {
            if (e.IsMainFrame)
            {
                string url = string.Format("{0}", e.Url);
                Program.PrintCtrl(wcAwesomium, string.Format(@"form-main-{0:yyMMdd-HHmmss-fff}.png", DateTime.Now));
                if ((CheckUrlDefault(url) || CheckUrlLiveDealer(url)) && !CheckLogin())
                {
                    HdlLogin();
                }
                else if (CheckUrlLiveDealer(url))
                {
                    HdlOpenAG();
                }
                else if (CheckUrlDefault(url))
                {
                    HdlNavLiveDealer();
                }
            }
        }
        #endregion
        #region For: Methods
        private void HdlLogin()
        {
            #region Script content
            string script = @"
(function($) {
	$(""#matterhorn-username"").val(""{$USERNAME}"");
	$(""#matterhorn-password"").val(""{$PASSWORD}"");
	$(""#account-login-submit"").trigger(""click"");
})(jQuery);".Replace("{$USERNAME}", Program.Config.CONFIG_DAFANBA_USER).Replace("{$PASSWORD}", Program.Config.CONFIG_DAFANBA_PASS);
            #endregion
            wcAwesomium.ExecuteJavascript(script);
            if (Error.None != wcAwesomium.GetLastError())
            {
                Program.Config.Log.Log(string.Format("Information\t:: There was a error calling this synchronous method | {0}", wcAwesomium.GetLastError()));
            }
        }

        private void HdlOpenAG()
        {
            #region Script content
            string script = @"
newwindow = window.open(""http://cdn.media.dafatouzhu.org/prod/macau-live/loader/l.html?t=30&l="", ""AGGameWindow"", ""width=3840,height=2160,top=0,left=0,toolbar=0,location=0,status=0,menubar=0,scrollbars=1,resizable=1"");
jQuery.get(""/vn/live-dealer/icore/get-lobby-url/ag"", function(data) {
	"""" != data.url && newwindow.location.replace(data.url)
});";
            #endregion
            wcAwesomium.ExecuteJavascript(script);
            if (Error.None != wcAwesomium.GetLastError())
            {
                Program.Config.Log.Log(string.Format("Information\t:: There was a error calling this synchronous method | {0}", wcAwesomium.GetLastError()));
            }
        }

        private void HdlNavLiveDealer()
        {
            #region Script content
            string script = @"
(function($) {
	$("".nav-product a"").each(function (index, element) {
		var $this = $(element);
		var href = $(this).attr(""href"");
		if (/^https:\/\/www\.dafanba\.org\/vn\/live-dealer.+$/gi.test(href)) {
			location.href = href;
		}
	});
})(jQuery);";
            #endregion
            wcAwesomium.ExecuteJavascript(script);
            if (Error.None != wcAwesomium.GetLastError())
            {
                Program.Config.Log.Log(string.Format("Information\t:: There was a error calling this synchronous method | {0}", wcAwesomium.GetLastError()));
            }
        }

        private bool CheckLogin()
        {
            #region Script content
            string script = @"
(function (d) { return d(); })(function () {
	return null == document.getElementById(""account-login-submit"");
});";
            #endregion
            JSValue js_val = wcAwesomium.ExecuteJavascriptWithResult(script);
            if (Error.None != wcAwesomium.GetLastError())
            {
                Program.Config.Log.Log(string.Format("Information\t:: There was a error calling this synchronous method | {0}", wcAwesomium.GetLastError()));
            }
            return (bool)js_val;
        }

        private bool CheckUrlDefault(string url)
        {
            Regex regex = new Regex(@"^https:\/\/www\.dafanba\.org\/vn\/?$", RegexOptions.Compiled);
            return regex.IsMatch(url);
        }

        private bool CheckUrlLiveDealer(string url)
        {
            Regex regex = new Regex(@"^https:\/\/www.dafanba.org\/vn\/live-dealer\/?$", RegexOptions.Compiled);
            return regex.IsMatch(url);
        }
        #endregion
        #region For: Properties
        private BindingSource bindingSource;
        #endregion
        #region For: Utilities & Other
        [System.Runtime.InteropServices.DllImport("KERNEL32.DLL", EntryPoint = "SetProcessWorkingSetSize", SetLastError = true, CallingConvention = System.Runtime.InteropServices.CallingConvention.StdCall)]
        internal static extern bool SetProcessWorkingSetSize(IntPtr pProcess, int dwMinimumWorkingSetSize, int dwMaximumWorkingSetSize);

        [System.Runtime.InteropServices.DllImport("KERNEL32.DLL", EntryPoint = "GetCurrentProcess", SetLastError = true, CallingConvention = System.Runtime.InteropServices.CallingConvention.StdCall)]
        internal static extern IntPtr GetCurrentProcess();
        #endregion
    }
}
