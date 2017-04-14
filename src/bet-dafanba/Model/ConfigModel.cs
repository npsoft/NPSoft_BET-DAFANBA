using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using SpiralEdge.Helper;

namespace SpiralEdge.Model
{
    public class ConfigModel
    {
        #region For: Configuration for connection
        public string CONFIG_CONN_STRING { get; set; }
        public int CONFIG_CONN_TIMEOUT { get; set; }
        public int CONFIG_CONN_TIMEOUT_TIMES { get; set; }
        public int CONFIG_CONN_TIMEOUT_INCREMENT { get; set; }
        public int CONFIG_CONN_PAGE_SIZE { get; set; }
        #endregion
        #region For: Configuration for path, email
        public string CONFIG_EMAIL_USER { get; set; }
        public string CONFIG_EMAIL_PASS { get; set; }
        public string[] CONFIG_EMAIL_RECEIVED { get; set; }
        public string[] CONFIG_EMAIL_RECEIVED_EX { get; set; }
        public string CONFIG_PATH_LOG { get; set; }
        public string CONFIG_PATH_CONFIG { get; set; }
        public DateTime CONFIG_REPORT_TIME { get; set; }
        #endregion
        #region For: Configuration for local, server
        public string CONFIG_DAFANBA_USER { get; set; }
        public string CONFIG_DAFANBA_PASS { get; set; }
        public string CONFIG_DAFANBA_URL_DEFAULT { get; set; }
        public string CONFIG_DAFANBA_URL_LIVEDEALER { get; set; }
        public string CONFIG_DAFANBA_DIR_PRINT { get; set; }
        public int CONFIG_DAFANBA_INTERVAL_CAPTURE_AG { get; set; }
        public int CONFIG_DAFANBA_ALERT_BACCARAT_PATTERN01 { get; set; }
        public int CONFIG_DAFANBA_ALERT_BACCARAT_PATTERN02 { get; set; }
        #endregion
        #region For: Configuration for schedule, settings
        #endregion
        #region For: Configuration for report, other settings
        public bool Test { get; set; }
        public RBLog Log { get; set; }
        public SQLiteHelper ConnHelper { get; set; }
        public List<DB_AGIN_Baccarat> LatestAGINs { get; set; }
        #endregion
        
        private void Init()
        {
            string s; DateTime dt;
            #region For: Initialize for connection
            CONFIG_CONN_STRING = ConfigurationManager.AppSettings["Conn_String"];
            CONFIG_CONN_TIMEOUT = int.Parse(ConfigurationManager.AppSettings["Conn_Timeout"]);
            CONFIG_CONN_TIMEOUT_TIMES = 3;
            CONFIG_CONN_TIMEOUT_INCREMENT = 5 * 60;
            CONFIG_CONN_PAGE_SIZE = int.Parse(ConfigurationManager.AppSettings["Conn_Page_Size"]);
            #endregion
            #region For: Initialize for path, email
            CONFIG_EMAIL_USER = ConfigurationManager.AppSettings["Email_User"];
            CONFIG_EMAIL_PASS = ConfigurationManager.AppSettings["Email_Pass"];
            CONFIG_EMAIL_RECEIVED = ConfigurationManager.AppSettings["Email_Recieved"].Split(new string[1] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            CONFIG_EMAIL_RECEIVED_EX = ConfigurationManager.AppSettings["Email_Recieved_Ex"].Split(new string[1] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            CONFIG_PATH_LOG = ConfigurationManager.AppSettings["Path_Log"];
            CONFIG_PATH_CONFIG = ConfigurationManager.AppSettings["Path_Config"];
            CONFIG_REPORT_TIME = DateTime.Now;
            s = ConfigurationManager.AppSettings["Report_Time"];
            if (!string.IsNullOrEmpty(s))
            {
                DateTime.TryParseExact(s, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out dt);
                CONFIG_REPORT_TIME = dt;
            }
            #endregion
            #region For: Initialize for local, server
            CONFIG_DAFANBA_USER = ConfigurationManager.AppSettings["Dafanba_User"];
            CONFIG_DAFANBA_PASS = ConfigurationManager.AppSettings["Dafanba_Pass"];
            CONFIG_DAFANBA_URL_DEFAULT = ConfigurationManager.AppSettings["Dafanba_Url_Default"];
            CONFIG_DAFANBA_URL_LIVEDEALER = ConfigurationManager.AppSettings["Dafanba_Url_LiveDealer"];
            CONFIG_DAFANBA_DIR_PRINT = ConfigurationManager.AppSettings["Dafanba_Dir_Print"];
            CONFIG_DAFANBA_INTERVAL_CAPTURE_AG = int.Parse(ConfigurationManager.AppSettings["Dafanba_Interval_Capture_AG"]) * 1000;
            CONFIG_DAFANBA_ALERT_BACCARAT_PATTERN01 = int.Parse(ConfigurationManager.AppSettings["Dafanba_Alert_Baccarat_Pattern01"]);
            CONFIG_DAFANBA_ALERT_BACCARAT_PATTERN02 = int.Parse(ConfigurationManager.AppSettings["Dafanba_Alert_Baccarat_Pattern02"]);
            #endregion
            #region For: Initialize for schedule, settings
            #endregion
            #region For: Initialize for report, other settings
            Test = "1" == ConfigurationManager.AppSettings["Test"];
            Log = new RBLog(CONFIG_PATH_LOG);
            ConnHelper = new SQLiteHelper(CONFIG_CONN_STRING);
            InitAGINs();
            Log.Log(string.Format("Information\t:: ------------------ APPLICATION STARTING -----------------------"));
            #endregion
        }

        private void InitAGINs()
        {
            LatestAGINs = new List<DB_AGIN_Baccarat>();
            SQLiteCommand cmd = ConnHelper.ConnDb.CreateCommand();
            try
            {
                #region SQLiteCommand: Initialize
                #region cmd.CommandText = string.Format(@"")
                cmd.CommandText = string.Format(@"
SELECT A.*
FROM AGIN A
WHERE Id IN (SELECT MAX(Id) FROM AGIN GROUP BY CoordinateX, CoordinateY)");
                #endregion
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = CONFIG_CONN_TIMEOUT;
                #endregion
                #region SQLiteCommand: Parameters
                #endregion
                #region SQLiteCommand: Connection
                DataSet ds = ConnHelper.ExecCmd(cmd);
                #endregion
                #region For: Retrieve
                DataTable dt = ds.Tables[0];
                List<string> cols = ToCols(dt);
                foreach (DataRow dr in dt.Rows)
                {
                    LatestAGINs.Add(DB_AGIN_Baccarat.ExtractDB(dr, cols));
                }
                #endregion
                #region For: Clean
                dt.Clear();
                ds.Clear();
                dt.Dispose();
                ds.Dispose();
                #endregion
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("{0}{1}", ex.Message, ex.StackTrace), ex);
            }
            finally
            {
                cmd.Dispose();
            }
        }
        
        public ConfigModel() { }

        public ConfigModel(bool isInit = true)
        {
            if (isInit) { Init(); }
        }

        public void HdlAGIN(string name)
        {
            try
            {
                string file_path = Path.Combine(CONFIG_DAFANBA_DIR_PRINT, name);
                if (File.Exists(file_path))
                {
                    AGIN_3840x2160_Baccarat agin_3840x2160_baccarat = null;
                    ImageHelper.AnalysisImg_AGIN_3840x2160(file_path, out agin_3840x2160_baccarat);

                    List<DB_AGIN_Baccarat> agins_img = DB_AGIN_Baccarat.ExtractImg(agin_3840x2160_baccarat, name, DateTime.Now, 0, DateTime.Now, 0);
                    foreach (DB_AGIN_Baccarat agin_img in agins_img)
                    {
                        DB_AGIN_Baccarat agin_latest = LatestAGINs.Where(x => x.CoordinateX == agin_img.CoordinateX && x.CoordinateY == agin_img.CoordinateY).FirstOrDefault();
                        #region For: Save/Clean baccarat
                        if (!new int[2] { 0, 204 }.Contains(agin_img.DataAnalysis.TotalInvalid))
                        {
                            agin_img.SaveDbTrack(ConnHelper);
                        }
                        if (0 != agin_img.DataAnalysis.TotalInvalid)
                        {
                            continue;
                        }
                        agin_img.Id = 0;
                        agin_img.DataAnalysis.DelEmpty();
                        #endregion
                        #region For: Merge baccarat
                        bool merged = false;
                        if (null != agin_latest)
                        {
                            int dist = DB_AGIN_Baccarat_Tbl.DistMerge(
                                agin_latest.DataAnalysis, agin_img.DataAnalysis,
                                DB_AGIN_Baccarat_Tbl.DistMax(agin_latest.DataAnalysis, agin_img.DataAnalysis));
                            if (-1 != dist)
                            {
                                DB_AGIN_Baccarat_Tbl.ExecMerge(agin_latest.DataAnalysis, agin_img.DataAnalysis, dist);
                                agin_latest.FileNames = Regex.Replace(agin_latest.FileNames + agin_img.FileNames, @"(;;)", ";");
                                agin_latest.LastModifiedOn = agin_img.LastModifiedOn;
                                agin_latest.LastModifiedBy = agin_img.LastModifiedBy;
                                merged = true;
                            }
                        }
                        #endregion
                        #region For: Add baccarat
                        if (!merged)
                        {
                            if (null != agin_latest)
                            {
                                LatestAGINs.Remove(agin_latest);
                            }
                            LatestAGINs.Add(agin_img);
                            agin_latest = LatestAGINs.Single(x => x.CoordinateX == agin_img.CoordinateX && x.CoordinateY == agin_img.CoordinateY);
                        }
                        #endregion
                        #region For: Order baccarat
                        agin_latest.DataAnalysis.UpdOrder(
                            agin_latest.DataAnalysis.LatestOrder, agin_latest.DataAnalysis.LatestOrderCircle,
                            agin_latest.DataAnalysis.LatestOrderX, agin_latest.DataAnalysis.LatestOrderY,
                            agin_latest.DataAnalysis.LatestOrderXR, agin_latest.DataAnalysis.LatestOrderYR);
                        #endregion
                        #region For: Alert via pattern(s)
                        #region For: Baccarat pattern #01
                        int pattern01 = agin_latest.ChkPattern01();
                        if (CONFIG_DAFANBA_ALERT_BACCARAT_PATTERN01 <= pattern01 && !agin_latest.AlertPattern01)
                        {
                            AlertBaccaratPattern01(agin_latest.CoordinateX, agin_latest.CoordinateY, agin_latest.DataAnalysis.LatestOrderCircle, pattern01, file_path);
                        }
                        agin_latest.AlertPattern01 = CONFIG_DAFANBA_ALERT_BACCARAT_PATTERN01 <= pattern01;
                        #endregion
                        #region For: Baccarat pattern #02
                        Tuple<int, string, int, string, int> pattern02 = agin_latest.ChkPattern02();
                        if (CONFIG_DAFANBA_ALERT_BACCARAT_PATTERN02 <= pattern02.Item1 && !agin_latest.AlertPattern02)
                        {
                            AlertBaccaratPattern02(agin_latest.CoordinateX, agin_latest.CoordinateY, pattern02.Item2, pattern02.Item3, pattern02.Item4, pattern02.Item5, pattern02.Item1, file_path);
                        }
                        agin_latest.AlertPattern02 = CONFIG_DAFANBA_ALERT_BACCARAT_PATTERN02 <= pattern02.Item1 || CONFIG_DAFANBA_ALERT_BACCARAT_PATTERN02 <= agin_latest.ChkPattern02(1).Item1 || CONFIG_DAFANBA_ALERT_BACCARAT_PATTERN02 <= agin_latest.ChkPattern02(2).Item1;
                        #endregion
                        #endregion
                        #region For: Save data to database
                        agin_latest.SaveDb(ConnHelper);
                        #endregion
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("{0}{1}", ex.Message, ex.StackTrace), ex);
            }
        }

        public void AlertBaccaratPattern01(int x, int y, string type, int length, string attachment)
        {
            Log.Log(string.Format("Information\t:: [BACCARAT PATTERN 01] Alert processing has been started."));
            #region For: Write log
            Log.Log(string.Format("Information\t:: [ coordinate = ({0},{1}), type = {2}, length = {3}, attachment = {4} ]", x, y, type, length, attachment));
            #endregion
            #region For: Send email(s)
            string subject = string.Format("[BET - AGIN - 01] ({0},{1}) = {2} | {3:yyyy-MM-dd HH:mm:ss}", x, y, length, DateTime.Now);
            string content = string.Format(@"
            Hi you,<br/><br/>
            Please review alert for AGIN | {4:yyyy-MM-dd HH:mm:ss}:<br/>
            <ul style='padding:0'>
                <li>[ coordinate = ({0},{1}), type = {2}, length = {3} ]</li>
            </ul><br/>
            Best regards!", x, y, type, length, DateTime.Now);
            string[] attachments = new string[1] { attachment };
            string display_name = "BET TOOL";
            MailHelper mail_helper = new MailHelper(CONFIG_EMAIL_USER, CONFIG_EMAIL_PASS);
            string send_email = mail_helper.SendEmail(CONFIG_EMAIL_RECEIVED, subject, content, attachments, display_name);
            Log.Log(string.Format("Information\t:: [ send email(s) = {0} ]", send_email));
            #endregion
            Log.Log(string.Format("Information\t:: [BACCARAT PATTERN 01] Alert processing has been completed."));
        }

        public void AlertBaccaratPattern02(int x, int y, string last_type, int last_len, string prev_type, int prev_len, int length, string attachment)
        {
            Log.Log(string.Format("Information\t:: [BACCARAT PATTERN 02] Alert processing has been started."));
            #region For: Write log
            Log.Log(string.Format("Information\t:: [ coordinate = ({0},{1}), last = ({2},{3}), previous = ({4},{5}), length = {6}, attachment = {7} ]", x, y, last_type, last_len, prev_type, prev_len, length, attachment));
            #endregion
            #region For: Send email(s)
            string subject = string.Format("[BET - AGIN - 02] ({0},{1}) = {2} | {3:yyyy-MM-dd HH:mm:ss}", x, y, length, DateTime.Now);
            string content = string.Format(@"
            Hi you,<br/><br/>
            Please review alert for AGIN | {7:yyyy-MM-dd HH:mm:ss}:<br/>
            <ul style='padding:0'>
                <li>[ coordinate = ({0},{1}), last = ({2},{3}), previous = ({4},{5}), length = {6} ]</li>
            </ul><br/>
            Best regards!", x, y, last_type, last_len, prev_type, prev_len, length, DateTime.Now);
            string[] attachments = new string[1] { attachment };
            string display_name = "BET TOOL";
            MailHelper mail_helper = new MailHelper(CONFIG_EMAIL_USER, CONFIG_EMAIL_PASS);
            string send_email = mail_helper.SendEmail(CONFIG_EMAIL_RECEIVED, subject, content, attachments, display_name);
            Log.Log(string.Format("Information\t:: [ send email(s) = {0} ]", send_email));
            #endregion
            Log.Log(string.Format("Information\t:: [BACCARAT PATTERN 02] Alert processing has been completed."));
        }

        public void SendEmail(string mainContent = null)
        {
            Log.Log(string.Format("Information\t:: Send email(s) processing has been started."));
            string subject = string.Format("[BET - TOOL] REPORT | {0:yyyy-MM-dd HH:mm:ss}", CONFIG_REPORT_TIME);
            string content = string.Format(@"
            Hi you,<br/><br/>
            Please review report for {0:yyyy-MM-dd HH:mm:ss}:<br/><ul style='padding:0;'>", CONFIG_REPORT_TIME);
            content += string.Format("{0}", mainContent);
            content += string.Format("</ul><br/>Best regards!");
            string[] attachments = null;
            string display_name = "BET TOOL";
            MailHelper mail_helper = new MailHelper(CONFIG_EMAIL_USER, CONFIG_EMAIL_PASS);
            string send_email = mail_helper.SendEmail(CONFIG_EMAIL_RECEIVED, subject, content, attachments, display_name);
            Log.Log(string.Format("Information\t:: Send email(s) processing has been completed."));
        }
        
        public void Analysis1_AGIN()
        {
            List<DB_AGIN_Baccarat> agins = new List<DB_AGIN_Baccarat>();
            SQLiteCommand cmd = ConnHelper.ConnDb.CreateCommand();
            try
            {
                #region SQLiteCommand: Initialize
                #region cmd.CommandText = string.Format(@"")
                cmd.CommandText = string.Format(@"
SELECT ASUM.* FROM AGIN_SUMMARY ASUM ORDER BY ASUM.Id ASC");
                #endregion
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = CONFIG_CONN_TIMEOUT;
                #endregion
                #region SQLiteCommand: Parameters
                #endregion
                #region SQLiteCommand: Connection
                DataSet ds = ConnHelper.ExecCmd(cmd);
                #endregion
                #region For: Retrieve
                DataTable dt = ds.Tables[0];
                List<string> cols = ToCols(dt);
                foreach (DataRow dr in dt.Rows)
                {
                    agins.Add(DB_AGIN_Baccarat.ExtractDB(dr, cols));
                }
                #endregion
                #region For: Clean
                dt.Clear();
                ds.Clear();
                dt.Dispose();
                ds.Dispose();
                #endregion
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("{0}{1}", ex.Message, ex.StackTrace), ex);
            }
            finally
            {
                cmd.Dispose();
            }
            int pattern01_min = 5, pattern02_min = 2, idx = 1;
            foreach (DB_AGIN_Baccarat agin in agins)
            {
                int order = 0;
                int pattern01_prev_len = 0, pattern02_prev_len = 0;
                while (agin.DataAnalysis.LatestOrder > order++)
                {
                    DB_AGIN_Baccarat baccarat = agin.Clone();
                    baccarat.DataAnalysis.DelOrder(order);

                    int pattern01 = baccarat.ChkPattern01();
                    if (pattern01_min - 1 < pattern01)
                    {
                        baccarat.SaveDbResult1("pattern-01", pattern01, order, ConnHelper);
                    }
                    pattern01_prev_len = pattern01;

                    Tuple<int, string, int, string, int> pattern02 = baccarat.ChkPattern02();
                    if (pattern02_min > pattern02.Item1)
                    {
                        pattern02 = baccarat.ChkPattern02(1);
                        if (pattern02_min > pattern02.Item1)
                        {
                            pattern02 = baccarat.ChkPattern02(2);
                        }
                    }
                    if (pattern02_min - 1 < pattern02.Item1 && pattern02_prev_len < pattern02.Item1)
                    {
                        baccarat.SaveDbResult1("pattern-02", pattern02.Item1, order, ConnHelper);
                    }
                    pattern02_prev_len = pattern02.Item1;
                }
                System.Threading.Thread.Sleep(0);
                System.Windows.Forms.Application.DoEvents();
                Console.WriteLine(string.Format("Information\t:: {0:P}", (double)idx++ / agins.Count));
            }
        }

        public void Analysis2_AGIN()
        {
            List<DB_AGIN_Baccarat> agins = new List<DB_AGIN_Baccarat>();
            SQLiteCommand cmd = ConnHelper.ConnDb.CreateCommand();
            try
            {
                #region SQLiteCommand: Initialize
                #region cmd.CommandText = string.Format(@"")
                cmd.CommandText = string.Format(@"
SELECT ASUM.* FROM AGIN_SUMMARY ASUM ORDER BY ASUM.Id ASC");
                #endregion
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = CONFIG_CONN_TIMEOUT;
                #endregion
                #region SQLiteCommand: Parameters
                #endregion
                #region SQLiteCommand: Connection
                DataSet ds = ConnHelper.ExecCmd(cmd);
                #endregion
                #region For: Retrieve
                DataTable dt = ds.Tables[0];
                List<string> cols = ToCols(dt);
                foreach (DataRow dr in dt.Rows)
                {
                    agins.Add(DB_AGIN_Baccarat.ExtractDB(dr, cols));
                }
                #endregion
                #region For: Clean
                dt.Clear();
                ds.Clear();
                dt.Dispose();
                ds.Dispose();
                #endregion
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("{0}{1}", ex.Message, ex.StackTrace), ex);
            }
            finally
            {
                cmd.Dispose();
            }
            int order_min = 1, idx = 1;
            foreach (DB_AGIN_Baccarat agin in agins)
            {
                int order = order_min - 1;
                while (agin.DataAnalysis.LatestOrder > order++)
                {
                    List<DB_AGIN_Baccarat_Cell> cells = new List<DB_AGIN_Baccarat_Cell>();
                    agin.DataAnalysis.Cells.ForEach(x => {
                        cells.AddRange(x.Where(y => order + 1 > y.Order));
                    });
                    agin.SaveDbResult2(order, cells.Count(x => x.Matches.Contains("circle-red")), cells.Count(x => x.Matches.Contains("circle-blue")), ConnHelper);
                }
                System.Threading.Thread.Sleep(0);
                System.Windows.Forms.Application.DoEvents();
                Console.Clear();
                Console.WriteLine(string.Format("Information\t:: {0:P}", (double)idx++ / agins.Count));
            }
        }

        public static List<string> ToCols(DataTable dt)
        {
            List<string> cols = new List<string>();
            foreach (DataColumn dc in dt.Columns)
            {
                cols.Add(dc.ColumnName);
            }
            return cols;
        }

        public static void SendEmailEx(ConfigModel config, Exception ex)
        {
            try
            {
                string user = null != config ? config.CONFIG_EMAIL_USER : "{$USER}";
                string pass = null != config ? config.CONFIG_EMAIL_PASS : "{$PASS}";
                string[] emails = config != null ? config.CONFIG_EMAIL_RECEIVED_EX : new string[1] { "npe.etc@gmail.com" };
                string subject = string.Format("[BET - TOOL] EXCEPTION | {0:yyyy-MM-dd HH:mm:ss}", DateTime.Now);
                string content = string.Format(@"
                Hi you,<br/><br/>
                Important exception occurred, here is content:<br/><br/>
                Message: {0}<br/><br/>
                StackTrace: {1}<br/><br/>
                Please review,", ex.Message, ex.StackTrace);
                string[] attachments = null;
                string display_name = "BET TOOL";
                MailHelper mail_helper = new MailHelper(user, pass);
                string send_email = mail_helper.SendEmail(emails, subject, content, attachments, display_name);
            }
            catch { }
        }
    }
}
