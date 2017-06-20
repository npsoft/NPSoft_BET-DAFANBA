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
        public dynamic CONFIG_DAFANBA_ALERT_BACCARAT_PATTERN { get; set; }
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
            CONFIG_DAFANBA_ALERT_BACCARAT_PATTERN = JsonConvert.DeserializeObject(ConfigurationManager.AppSettings["Dafanba_Alert_Baccarat_Pattern"]);
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
                        if (!new int[3] { 0, 203, 204 }.Contains(agin_img.DataAnalysis.TotalInvalid))
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
                        #region For: Save baccarat
                        agin_latest.SaveDb(ConnHelper);
                        #endregion
                        #region For: Alert via pattern(s)
                        #region For: Baccarat pattern
                        IEnumerable<DB_AGIN_Baccarat_Check_RstItem> pattern = agin_latest.ChkPattern(CONFIG_DAFANBA_ALERT_BACCARAT_PATTERN);
                        foreach (DB_AGIN_Baccarat_Check_RstItem item in pattern)
                        {
                            AlertBaccaratPattern(agin_latest.CoordinateX, agin_latest.CoordinateY, item.NFreq, item.CellsFreq.Count, item.CellsSub.Count, item.ColorsFreq, file_path);
                        }
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

        public void AlertBaccaratPattern(int x, int y, int freqN, int freqL, int freqLSub, string freqColors, string attachment)
        {
            Log.Log(string.Format("Information\t:: [BACCARAT PATTERN] Alert processing has been started."));
            #region For: Write log
            Log.Log(string.Format("Information\t:: [ coordinate = ({0},{1}), type = {2}{3}{4}, length = {5}, attachment = {6} ]",
                x, y, freqColors, "{" + freqN + "}", freqColors.Substring(0, freqLSub), freqN * freqL + freqLSub, attachment));
            #endregion
            #region For: Send email(s)
            string subject = string.Format("[AGIN] ({0},{1}) = {2}{3}{4} = {5} | {6:yyyy-MM-dd HH:mm:ss}",
                x, y, freqColors, "{" + freqN + "}", freqColors.Substring(0, freqLSub), freqN * freqL + freqLSub, DateTime.Now);
            string content = string.Format(@"
Hi you,<br/><br/>
Please review alert for AGIN | {6:yyyy-MM-dd HH:mm:ss}:<br/>
<ul style='padding:0'>
    <li>[ coordinate = ({0},{1}), type = {2}{3}{4}, length = {5} ]</li>
</ul><br/>
Best regards!",
x, y, freqColors, "{" + freqN + "}", freqColors.Substring(0, freqLSub), freqN * freqL + freqLSub, DateTime.Now);
            string[] attachments = new string[1] { attachment };
            string display_name = "BET TOOL";
            MailHelper mail_helper = new MailHelper(CONFIG_EMAIL_USER, CONFIG_EMAIL_PASS);
            string send_email = mail_helper.SendEmail(CONFIG_EMAIL_RECEIVED, subject, content, attachments, display_name);
            Log.Log(string.Format("Information\t:: [ send email(s) = {0} ]", send_email));
            #endregion
            Log.Log(string.Format("Information\t:: [BACCARAT PATTERN] Alert processing has been completed."));
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
SELECT ASUM.* FROM AGIN_SUMMARY ASUM WHERE ASUM.Id > 7713 ORDER BY ASUM.Id ASC");
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
                    DB_AGIN_Baccarat_Check baccarat_check = new DB_AGIN_Baccarat_Check(agin, 10, 1, new KeyValuePair<int, int>[10] {
                        new KeyValuePair<int, int>(1, 1),
                        new KeyValuePair<int, int>(2, 6),
                        new KeyValuePair<int, int>(3, 9),
                        new KeyValuePair<int, int>(4, 12),
                        new KeyValuePair<int, int>(5, 15),
                        new KeyValuePair<int, int>(6, 18),
                        new KeyValuePair<int, int>(7, 21),
                        new KeyValuePair<int, int>(8, 24),
                        new KeyValuePair<int, int>(9, 27),
                        new KeyValuePair<int, int>(10, 30)});
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
SELECT ASUM.* FROM AGIN_SUMMARY ASUM WHERE ASUM.Id > 7713 ORDER BY ASUM.Id ASC");
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

        public void Analysis3_AGIN()
        {
            SQLiteCommand cmd = ConnHelper.ConnDb.CreateCommand();
            try
            {
                #region SQLiteCommand: Initialize
                #region cmd.CommandText = string.Format(@"")
                cmd.CommandText = string.Format(@"
PRAGMA foreign_keys=off;
PRAGMA temp_store=2;
BEGIN TRANSACTION;

CREATE TEMP TABLE IF NOT EXISTS _Variables (Name TEXT PRIMARY KEY NOT NULL, Value TEXT);

DROP TABLE IF EXISTS tmpMaxOrder;
CREATE TEMPORARY TABLE tmpMaxOrder AS
WITH FT_CTE AS (
    SELECT SubId, MAX(LatestOrder) MaxOrder FROM AGIN_RESULT2 GROUP BY SubId)
SELECT * FROM FT_CTE;

DROP TABLE IF EXISTS tmpDistAVG;
CREATE TEMPORARY TABLE tmpDistAVG AS
WITH FT_CTE AS (
    SELECT ASUM.Id
        , MO.MaxOrder
        , CAST(60 AS INT) DistAVG
        , ASUM.LastModifiedOn LastModified
        , strftime('%s', ASUM.LastModifiedOn) LastModifiedUnix
    FROM AGIN_SUMMARY ASUM
        INNER JOIN tmpMaxOrder MO ON MO.SubId = ASUM.Id)
SELECT * FROM FT_CTE;

DROP TABLE IF EXISTS tmpAR1;
CREATE TEMPORARY TABLE tmpAR1 AS
WITH FT_CTE AS (
    SELECT AR.Id, AR.SubId, AR.LatestOrder
        , AR.FreqN, AR.FreqL, AR.FreqLSub, AR.FreqColors
        , AR.FreqN * AR.FreqL + AR.FreqLSub FreqLTotal
        , ((DA.LastModifiedUnix - (DA.MaxOrder - AR.LatestOrder) * DA.DistAVG) / 1800) * 1800 LastModifiedUnix
    FROM AGIN_RESULT1 AR
        INNER JOIN tmpDistAVG DA ON DA.Id = AR.SubId)
SELECT * FROM FT_CTE;

INSERT OR REPLACE INTO _Variables VALUES ('min-latest-modified-unix', (SELECT MIN(LastModifiedUnix) FROM tmpAR1));
INSERT OR REPLACE INTO _Variables VALUES ('max-latest-modified-unix', (SELECT MAX(LastModifiedUnix) FROM tmpAR1));
UPDATE _Variables SET Value =
    ((SELECT CAST(COALESCE(Value, NULL) AS INT) FROM _Variables WHERE Name = 'min-latest-modified-unix' LIMIT 1) / 1800) * 1800
WHERE Name = 'min-latest-modified-unix';
UPDATE _Variables SET Value = (CASE
    WHEN CAST((SELECT CAST(COALESCE(Value, NULL) AS INT) FROM _Variables WHERE Name = 'max-latest-modified-unix' LIMIT 1) AS DOUBLE) / 60 <> 0 THEN
        ((SELECT CAST(COALESCE(Value, NULL) AS INT) FROM _Variables WHERE Name = 'max-latest-modified-unix' LIMIT 1) / 1800 + 1) * 1800
    ELSE
        (SELECT CAST(COALESCE(Value, NULL) AS INT) FROM _Variables WHERE Name = 'max-latest-modified-unix' LIMIT 1)
    END)
WHERE Name = 'max-latest-modified-unix';
DROP TABLE IF EXISTS tmpTime;
CREATE TEMPORARY TABLE tmpTime AS
WITH FT_CTE AS (
    WITH RECURSIVE recursiveTime (time)
    AS (
        SELECT (SELECT CAST(COALESCE(Value, NULL) AS INT) FROM _Variables WHERE Name = 'min-latest-modified-unix' LIMIT 1)
        UNION ALL
        SELECT time + 1800 FROM recursiveTime WHERE time < (SELECT CAST(COALESCE(Value, NULL) AS INT) FROM _Variables WHERE Name = 'max-latest-modified-unix' LIMIT 1))
    SELECT datetime(time, 'unixepoch') LastModified, time LastModifiedUnix FROM recursiveTime)
SELECT * FROM FT_CTE;

DROP TABLE IF EXISTS tmpAR1Cus;
CREATE TEMPORARY TABLE tmpAR1Cus AS
WITH FT_CTE AS (
    SELECT AR.FreqL, AR.FreqLTotal, T.LastModified, COUNT(1) Times
    FROM tmpAR1 AR
        INNER JOIN tmpTime T ON AR.LastModifiedUnix = T.LastModifiedUnix
    GROUP BY AR.FreqL, AR.FreqLTotal, T.LastModified)
SELECT * FROM FT_CTE;

COMMIT;
PRAGMA foreign_keys=on;
SELECT * FROM tmpAR1Cus;");
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
                DataTable dt = ds.Tables[0].Copy();
                string path = Path.Combine(CONFIG_PATH_CONFIG, string.Format("analysis3-agin-{0:yyMMddHHmmss}.js", CONFIG_REPORT_TIME));
                FileHelper.WriteFileWithSW(path, "var data = " + JsonConvert.SerializeObject(dt) + ";");
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
