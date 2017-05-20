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
using Newtonsoft.Json;
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
        public dynamic CONFIG_DAFANBA_ALERT_BACCARAT_PATTERN03 { get; set; }
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
            CONFIG_DAFANBA_ALERT_BACCARAT_PATTERN03 = JsonConvert.DeserializeObject(ConfigurationManager.AppSettings["Dafanba_Alert_Baccarat_Pattern03"]);
            CONFIG_DAFANBA_ALERT_BACCARAT_PATTERN03["p-circle-rb-min"] = (double)JsonHelper.GetElBySelector(CONFIG_DAFANBA_ALERT_BACCARAT_PATTERN03, "p-circle-rb-min-n").Value / JsonHelper.GetElBySelector(CONFIG_DAFANBA_ALERT_BACCARAT_PATTERN03, "p-circle-rb-min-d").Value;
            CONFIG_DAFANBA_ALERT_BACCARAT_PATTERN03["p-circle-rb-max"] = (double)JsonHelper.GetElBySelector(CONFIG_DAFANBA_ALERT_BACCARAT_PATTERN03, "p-circle-rb-max-n").Value / JsonHelper.GetElBySelector(CONFIG_DAFANBA_ALERT_BACCARAT_PATTERN03, "p-circle-rb-max-d").Value;
            CONFIG_DAFANBA_ALERT_BACCARAT_PATTERN03["p-circle-br-min"] = (double)JsonHelper.GetElBySelector(CONFIG_DAFANBA_ALERT_BACCARAT_PATTERN03, "p-circle-br-min-n").Value / JsonHelper.GetElBySelector(CONFIG_DAFANBA_ALERT_BACCARAT_PATTERN03, "p-circle-br-min-d").Value;
            CONFIG_DAFANBA_ALERT_BACCARAT_PATTERN03["p-circle-br-max"] = (double)JsonHelper.GetElBySelector(CONFIG_DAFANBA_ALERT_BACCARAT_PATTERN03, "p-circle-br-max-n").Value / JsonHelper.GetElBySelector(CONFIG_DAFANBA_ALERT_BACCARAT_PATTERN03, "p-circle-br-max-d").Value;
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
                                #region For: Alert | Baccarat pattern #03 | Fail
                                Tuple<string, DB_AGIN_Baccarat_Cell, int, int> pattern03_fail = agin_latest.ChkPattern03(CONFIG_DAFANBA_ALERT_BACCARAT_PATTERN03, int.MaxValue);
                                if (!string.IsNullOrEmpty(pattern03_fail.Item1) && agin_latest.AlertPattern03)
                                {
                                    AlertBaccaratPattern03_Fail(agin_latest.CoordinateX, agin_latest.CoordinateY, pattern03_fail.Item3, pattern03_fail.Item4, agin_latest.Id,
                                        Path.Combine(CONFIG_DAFANBA_DIR_PRINT, agin_latest.FileNames.Split(new string[1] { ";" }, StringSplitOptions.RemoveEmptyEntries).Last()));
                                }
                                #endregion
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
                        #region For: Save baccarat
                        agin_latest.SaveDb(ConnHelper);
                        #endregion
                        #region For: Alert via pattern(s)
                        #region For: Baccarat pattern #01
                        Tuple<int, DB_AGIN_Baccarat_Cell> pattern01 = agin_latest.ChkPattern01(int.MaxValue);
                        if (CONFIG_DAFANBA_ALERT_BACCARAT_PATTERN01 <= pattern01.Item1 && !agin_latest.AlertPattern01)
                        {
                            AlertBaccaratPattern01(agin_latest.CoordinateX, agin_latest.CoordinateY, pattern01.Item2.CircleColor, pattern01.Item1, file_path);
                        }
                        agin_latest.AlertPattern01 = CONFIG_DAFANBA_ALERT_BACCARAT_PATTERN01 <= pattern01.Item1;
                        #endregion
                        #region For: Baccarat pattern #02
                        Tuple<int, DB_AGIN_Baccarat_Cell, int, DB_AGIN_Baccarat_Cell, int> pattern02 = agin_latest.ChkPattern02(0, int.MaxValue);
                        if (CONFIG_DAFANBA_ALERT_BACCARAT_PATTERN02 <= pattern02.Item1 && !agin_latest.AlertPattern02)
                        {
                            AlertBaccaratPattern02(agin_latest.CoordinateX, agin_latest.CoordinateY, pattern02.Item2.CircleColor, pattern02.Item3, pattern02.Item4.CircleColor, pattern02.Item5, pattern02.Item1, file_path);
                        }
                        agin_latest.AlertPattern02 = CONFIG_DAFANBA_ALERT_BACCARAT_PATTERN02 <= pattern02.Item1 || CONFIG_DAFANBA_ALERT_BACCARAT_PATTERN02 <= agin_latest.ChkPattern02(1, int.MaxValue).Item1 || CONFIG_DAFANBA_ALERT_BACCARAT_PATTERN02 <= agin_latest.ChkPattern02(2, int.MaxValue).Item1;
                        #endregion
                        #region For: Baccarat pattern #03
                        Tuple<string, DB_AGIN_Baccarat_Cell, int, int> pattern03 = agin_latest.ChkPattern03(CONFIG_DAFANBA_ALERT_BACCARAT_PATTERN03, int.MaxValue);
                        if (!string.IsNullOrEmpty(pattern03.Item1) && !agin_latest.AlertPattern03)
                        {
                            AlertBaccaratPattern03_Open(agin_latest.CoordinateX, agin_latest.CoordinateY, pattern03.Item2.CoordinateX, pattern03.Item2.CoordinateY, pattern03.Item3, pattern03.Item4, agin_latest.Id, pattern03.Item1, file_path);
                        }
                        if (string.IsNullOrEmpty(pattern03.Item1) && agin_latest.AlertPattern03)
                        {
                            AlertBaccaratPattern03_Close(agin_latest.CoordinateX, agin_latest.CoordinateY, pattern03.Item2.CoordinateX, pattern03.Item2.CoordinateY, pattern03.Item3, pattern03.Item4, agin_latest.Id, file_path);
                        }
                        agin_latest.AlertPattern03 = !string.IsNullOrEmpty(pattern03.Item1);
                        #endregion
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
            string subject = string.Format("[AGIN - 01] ({0},{1}) = {2} | {3:yyyy-MM-dd HH:mm:ss}", x, y, length, DateTime.Now);
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

        public void AlertBaccaratPattern02(int x, int y, string lastType, int lastLen, string prev_type, int prev_len, int length, string attachment)
        {
            Log.Log(string.Format("Information\t:: [BACCARAT PATTERN 02] Alert processing has been started."));
            #region For: Write log
            Log.Log(string.Format("Information\t:: [ coordinate = ({0},{1}), last = ({2},{3}), previous = ({4},{5}), length = {6}, attachment = {7} ]", x, y, lastType, lastLen, prev_type, prev_len, length, attachment));
            #endregion
            #region For: Send email(s)
            string subject = string.Format("[AGIN - 02] ({0},{1}) = {2} | {3:yyyy-MM-dd HH:mm:ss}", x, y, length, DateTime.Now);
            string content = string.Format(@"
Hi you,<br/><br/>
Please review alert for AGIN | {7:yyyy-MM-dd HH:mm:ss}:<br/>
<ul style='padding:0'>
    <li>[ coordinate = ({0},{1}), last = ({2},{3}), previous = ({4},{5}), length = {6} ]</li>
</ul><br/>
Best regards!", x, y, lastType, lastLen, prev_type, prev_len, length, DateTime.Now);
            string[] attachments = new string[1] { attachment };
            string display_name = "BET TOOL";
            MailHelper mail_helper = new MailHelper(CONFIG_EMAIL_USER, CONFIG_EMAIL_PASS);
            string send_email = mail_helper.SendEmail(CONFIG_EMAIL_RECEIVED, subject, content, attachments, display_name);
            Log.Log(string.Format("Information\t:: [ send email(s) = {0} ]", send_email));
            #endregion
            Log.Log(string.Format("Information\t:: [BACCARAT PATTERN 02] Alert processing has been completed."));
        }

        public void AlertBaccaratPattern03_Open(int x, int y, int lastX, int lastY, int numRed, int numBlue, long id, string color, string attachment)
        {
            Log.Log(string.Format("Information\t:: [BACCARAT PATTERN 03] OPEN - Alert processing has been started."));
            #region For: Write log
            Log.Log(string.Format("Information\t:: [ id = {0} → open → {1}, coordinate = ({2},{3}) → ({4},{5}), number-red = {6}, number-blue = {7}, attachment = {8} ]", id, color, x, y, lastX, lastY, numRed, numBlue, attachment));
            #endregion
            #region For: Send email(s)
            string subject = string.Format("[AGIN - 03] {0} | OPEN &rarr; {1} | ({2},{3}) &rarr; ({4},{5})", id, color, x, y, lastX, lastY);
            string content = string.Format(@"
Hi you,<br/><br/>
Please review alert for AGIN | {8:yyyy-MM-dd HH:mm:ss}:<br/>
<ul style='padding:0'>
    <li>[ id = {0} &rarr; open &rarr; {1}, coordinate = ({2},{3}) &rarr; ({4},{5}), number-red = {6}, number-blue = {7} ]</li>
</ul><br/>
Best regards!", id, color, x, y, lastX, lastY, numRed, numBlue, DateTime.Now);
            string[] attachments = new string[1] { attachment };
            string display_name = "BET TOOL";
            MailHelper mail_helper = new MailHelper(CONFIG_EMAIL_USER, CONFIG_EMAIL_PASS);
            string send_email = mail_helper.SendEmail(CONFIG_EMAIL_RECEIVED, subject, content, attachments, display_name);
            Log.Log(string.Format("Information\t:: [ send email(s) = {0} ]", send_email));
            #endregion
            Log.Log(string.Format("Information\t:: [BACCARAT PATTERN 03] OPEN - Alert processing has been completed."));
        }

        public void AlertBaccaratPattern03_Close(int x, int y, int lastX, int lastY, int numRed, int numBlue, long id, string attachment)
        {
            Log.Log(string.Format("Information\t:: [BACCARAT PATTERN 03] CLOSE - Alert processing has been started."));
            #region For: Write log
            Log.Log(string.Format("Information\t:: [ id = {0} → close, coordinate = ({1},{2}) → ({3},{4}), number-red = {5}, number-blue = {6}, attachment = {7} ]", id, x, y, lastX, lastY, numRed, numBlue, attachment));
            #endregion
            #region For: Send email(s)
            string subject = string.Format("[AGIN - 03] {0} | CLOSE | ({1},{2}) &rarr; ({3},{4})", id, x, y, lastX, lastY);
            string content = string.Format(@"
Hi you,<br/><br/>
Please review alert for AGIN | {7:yyyy-MM-dd HH:mm:ss}:<br/>
<ul style='padding:0'>
    <li>[ id = {0} &rarr; close, coordinate = ({1},{2}) &rarr; ({3},{4}), number-red = {5}, number-blue = {6} ]</li>
</ul>
Best regards!", id, x, y, lastX, lastY, numRed, numBlue, DateTime.Now);
            string[] attachments = new string[1] { attachment };
            string display_name = "BET TOOL";
            MailHelper mail_helper = new MailHelper(CONFIG_EMAIL_USER, CONFIG_EMAIL_PASS);
            string send_email = mail_helper.SendEmail(CONFIG_EMAIL_RECEIVED, subject, content, attachments, display_name);
            Log.Log(string.Format("Information\t:: [ send email(s) = {0} ]", send_email));
            #endregion
            Log.Log(string.Format("Information\t:: [BACCARAT PATTERN 03] CLOSE - Alert processing has been completed."));
        }

        public void AlertBaccaratPattern03_Fail(int x, int y, int numRed, int numBlue, long id, string attachment)
        {
            Log.Log(string.Format("Information\t:: [BACCARAT PATTERN 03] FAIL - Alert processing has been started."));
            #region For: Write log
            Log.Log(string.Format("Information\t:: [ id = {0} → fail, coordinate = ({1},{2}), number-red = {3}, number-blue = {4}, attachment = {5} ]", id, x, y, numRed, numBlue, attachment));
            #endregion
            #region For: Send email(s)
            string subject = string.Format("[AGIN - 03] {0} | FAIL | ({1},{2}) &rarr; {3}-r / {4}-b", id, x, y, numRed, numBlue);
            string content = string.Format(@"
Hi you,<br/><br/>
Please review alert for AGIN | {5:yyyy-MM-dd HH:mm:ss}:<br/>
<ul style='padding:0'>
    <li>[ id = {0} &rarr; fail, coordinate = ({1},{2}), number-red = {3}, number-blue = {4} ]</li>
</ul>
Best regards!", id, x, y, numRed, numBlue, DateTime.Now);
            string[] attachments = new string[1] { attachment };
            string display_name = "BET TOOL";
            MailHelper mail_helper = new MailHelper(CONFIG_EMAIL_USER, CONFIG_EMAIL_PASS);
            string send_email = mail_helper.SendEmail(CONFIG_EMAIL_RECEIVED, subject, content, attachments, display_name);
            Log.Log(string.Format("Information\t:: [ send email(s) = {0} ]", send_email));
            #endregion
            Log.Log(string.Format("Information\t:: [BACCARAT PATTERN 03] FAIL - Alert processing has been completed."));
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
            int idx = 1;
            List<List<object>> lst_vals = new List<List<object>>();
            string cmd_text = string.Format(@"INSERT INTO AGIN_RESULT1 (SubId, LatestOrder, FreqN, FreqL, FreqLSub, FreqColors) VALUES (?, ?, ?, ?, ?, ?)");
            foreach (DB_AGIN_Baccarat agin in agins)
            {
                agin.DataAnalysis.UpdCoordinate();
                int order = 0;
                while (agin.DataAnalysis.LatestOrder > order++)
                {
                    DB_AGIN_Baccarat_Check baccarat_check = new DB_AGIN_Baccarat_Check(agin, 9, 3, new KeyValuePair<int, int>[9] {
                        new KeyValuePair<int, int>(1, 5),
                        new KeyValuePair<int, int>(2, 6),
                        new KeyValuePair<int, int>(3, 9),
                        new KeyValuePair<int, int>(4, 12),
                        new KeyValuePair<int, int>(5, 15),
                        new KeyValuePair<int, int>(6, 18),
                        new KeyValuePair<int, int>(7, 21),
                        new KeyValuePair<int, int>(8, 24),
                        new KeyValuePair<int, int>(9, 27)});
                    baccarat_check.Search(order).ForEach(x => {
                        lst_vals.Add(new List<object>() { agin.Id, order, x.NFreq, x.CellsFreq.Count, x.CellsSub.Count, x.ColorsFreq });
                    });
                }
                System.Threading.Thread.Sleep(0);
                System.Windows.Forms.Application.DoEvents();
                Console.Clear();
                Console.WriteLine(string.Format("Information\t:: {0:P}", (double)idx++ / agins.Count));
            }
            ConnHelper.ExecNonQueryCmdOptimizeMany(lst_vals, cmd_text);
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
            List<List<object>> lst_vals = new List<List<object>>();
            string cmd_text = string.Format(@"INSERT INTO AGIN_RESULT2 (SubId, LatestOrder, NumCircleRed, NumCircleBlue, Matches) VALUES (?, ?, ?, ?, ?)");
            foreach (DB_AGIN_Baccarat agin in agins)
            {
                agin.DataAnalysis.UpdCoordinate();
                int order = order_min - 1;
                while (agin.DataAnalysis.LatestOrder > order++)
                {
                    List<DB_AGIN_Baccarat_Cell> cells = new List<DB_AGIN_Baccarat_Cell>();
                    agin.DataAnalysis.Cells.ForEach(x => {
                        cells.AddRange(x.Where(y => order + 1 > y.Order));
                    });
                    cells = cells.OrderBy(x => x.Order).ToList();
                    lst_vals.Add(new List<object>() {
                        agin.Id, order,
                        cells.Count(x => x.Matches.Contains("circle-red")),
                        cells.Count(x => x.Matches.Contains("circle-blue")),
                        string.Format(";{0};", string.Join(";", cells[cells.Count - 1].Matches)) });
                }
                System.Threading.Thread.Sleep(0);
                System.Windows.Forms.Application.DoEvents();
                Console.Clear();
                Console.WriteLine(string.Format("Information\t:: {0:P}", (double)idx++ / agins.Count));
            }
            ConnHelper.ExecNonQueryCmdOptimizeMany(lst_vals, cmd_text);
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
