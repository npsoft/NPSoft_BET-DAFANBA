using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using Newtonsoft.Json;
using SpiralEdge.Helper;

namespace SpiralEdge.Model
{
    public class DB_AGIN_Baccarat
    {
        #region For: Properties
        public long Id { get; set; }
        public int CoordinateX { get; set; }
        public int CoordinateY { get; set; }
        public string FileNames { get; set; }
        public List<List<DB_AGIN_Baccarat_Cell>> DataAnalysis { get; set; }
        public int DataTotalCol { get; set; }
        public int DataTotalRow { get; set; }
        public int DataTotalInvalid { get; set; }
        public int DataMaxIdxColRow0 { get; set; }
        public int DataLatestOrder { get; set; }
        public int DataLatestOrderX { get; set; }
        public int DataLatestOrderY { get; set; }
        public string DataLatestOrderColor { get; set; }
        public DateTime CreatedOn { get; set; }
        public long CreatedBy { get; set; }
        public DateTime LastModifiedOn { get; set; }
        public long LastModifiedBy { get; set; }
        #endregion
        #region For: Ctors
        public DB_AGIN_Baccarat() { }
        #endregion
        #region For: Methods
        public void UpdTotalColRow()
        {
            DataTotalCol = DataAnalysis.Count;
            DataTotalRow = 0 != DataTotalCol ? DataAnalysis[0].Count : 0;
        }

        public void UpdTotalInvalid()
        {
            DataTotalInvalid = 0;
            foreach (var cells in DataAnalysis)
            {
                foreach (var cell in cells)
                {
                    if (0 == cell.Matches.Count) { DataTotalInvalid++; }
                }
            }
        }

        public void UpdMaxIdxColRow0()
        {
            DataMaxIdxColRow0 = 0;
            for(int x = 0; x < DataAnalysis.Count; x++)
            {
                if (2 > DataAnalysis[x][0].Matches.Count)
                {
                    break;
                }
                DataMaxIdxColRow0++;
            }
        }

        public void RemoveEmptyCol()
        {
            for (int x = DataAnalysis.Count - 1; -1 < x; x--)
            {
                bool remove = true;
                foreach (var cell in DataAnalysis[x])
                {
                    if (1 < cell.Matches.Count)
                    {
                        remove = false; break;
                    }
                }
                if (remove) { DataAnalysis.RemoveAt(x); }
                else { break; }
            }
        }

        // Condition: DataTotalCol, DataTotalRow were up-to-date
        public void MarkOrderCells(int dataLatestOrder, int dataLatestOrderX, int dataLatestOrderY, string dataLatestOrderColor)
        {
            DataLatestOrder = dataLatestOrder;
            DataLatestOrderX = dataLatestOrderX;
            DataLatestOrderY = dataLatestOrderY;
            DataLatestOrderColor = dataLatestOrderColor;
            for (int x = 0; x < DataTotalCol; x++)
            {
                for (int y = 0; y < DataTotalRow; y++)
                {
                    if (x < DataLatestOrderX || x == DataLatestOrder && y < DataLatestOrderY) // For: 
                    {
                        continue;
                    }
                    var cell = DataAnalysis[x][y];
                    var set_order = false;
                    if (2 > cell.Matches.Count) // For: Current cell was empty, break loop
                    {
                        break;
                    }
                    if (0 == cell.Order && 0 == y) // For: Current cell was new, set order
                    {
                        set_order = true;
                        cell.Order = ++DataLatestOrder;
                        DataLatestOrderColor = cell.Matches.Contains("circle-blue") ? "circle-blue" : cell.Matches.Contains("circle-red") ? "circle-red" : "";
                    }
                    if (0 == cell.Order && cell.Matches.Contains(DataLatestOrderColor)) // For: Current cell was valid, set order
                    {
                        set_order = true;
                        cell.Order = ++DataLatestOrder;
                        DataLatestOrderColor = cell.Matches.Contains("circle-blue") ? "circle-blue" : "circle-red";
                    }
                    if (0 != cell.Order && !set_order || DataTotalRow - 1 == y && set_order) // For: Current cell was order or max index, consider right side
                    {
                        var y_r = 0 != cell.Order && !set_order ? y - 1 : y;
                        if (-1 != y_r) // For: Coordinate-y of right side was wrong, ignore this case
                        {
                            for (var x_r = x + 1; x_r < DataTotalCol; x_r++)
                            {
                                var cell_r = DataAnalysis[x_r][y_r];
                                if (!cell_r.Matches.Contains(DataLatestOrderColor)) // For: Right cell was invalid, break loop
                                {
                                    break;
                                }
                                var order_confuse = true; // For: Order can confuse, not sure about this check
                                for (var y_c = y_r - 1; y_c > -1; y_c++)
                                {
                                    var cell_c = DataAnalysis[x_r][y_c];
                                    if (!cell_c.Matches.Contains(DataLatestOrderColor) || x_r < DataTotalCol && DataAnalysis[x_r + 1][y_c].Matches.Contains(DataLatestOrderColor))
                                    {
                                        order_confuse = false;
                                        break;
                                    }
                                }
                                cell_r.Order = ++DataLatestOrder;
                                cell_r.OrderConfuse = order_confuse;
                            }
                        }
                        break;
                    }
                }
            }
        }
        
        public static int CheckMerge(DB_AGIN_Baccarat baccaratOrg, DB_AGIN_Baccarat baccaratNew, int distMax)
        {
            bool merged = false; int dist = 0;
            int padding = baccaratNew.DataTotalCol - baccaratOrg.DataTotalCol;
            for (int x_new = baccaratNew.DataTotalCol - 1; x_new  > - 1; x_new--)
            {
                // For: Determine distance, for new
                dist = baccaratNew.DataTotalCol - x_new - 1;
                if (distMax < dist) // For: Distance too far, not merge
                {
                    break;
                }
                // For: Determine min index, for origin
                int x_org_min = baccaratOrg.DataTotalCol - (baccaratNew.DataTotalCol - dist);
                if (0 > x_org_min) // For: Columns not enough, ignore it
                {
                    continue;
                }
                // For: Check cells were order, for origin
                bool flag = true;
                for (int x_org = baccaratOrg.DataTotalCol - 1; x_org > x_org_min - 1; x_org--)
                {
                    for (int y_org = baccaratOrg.DataTotalRow - 1; y_org > -1; y_org--)
                    {
                        var cell_org = baccaratOrg.DataAnalysis[x_org][y_org];
                        var cell_new = baccaratNew.DataAnalysis[x_org - padding][y_org];
                        string color_org = cell_org.Matches.Contains("circle-blue") ? "circle-blue" : cell_org.Matches.Contains("circle-red") ? "circle-red" : "";
                        string color_new = cell_new.Matches.Contains("circle-blue") ? "circle-blue" : cell_new.Matches.Contains("circle-red") ? "circle-red" : "";
                        if (0 != cell_org.Order && color_org != color_new) // For: Cell was order but different, ignore it
                        {
                            flag = false; break;
                        }
                    }
                    if (!flag) { break; }
                }
                if (flag) // For: Can merge with dist, for new
                {
                    merged = true; break;
                }
            }
            return merged ? dist : -1;
        }
        
        public static void ExecMerge(DB_AGIN_Baccarat baccaratOrg, DB_AGIN_Baccarat baccaratNew, int dist)
        {
            // For: Determine min index, for new
            int x_new_min = baccaratNew.DataTotalCol - dist - 1 - (baccaratOrg.DataTotalCol - baccaratOrg.DataMaxIdxColRow0 - 1);
            for (int x_new = x_new_min; x_new < baccaratNew.DataTotalCol; x_new++)
            {
                // For: Determine current index, for origin
                int x_org = baccaratOrg.DataMaxIdxColRow0 + (x_new - x_new_min);
                if (x_org > baccaratOrg.DataAnalysis.Count - 1) // For: Allocating to column, for origin
                {
                    baccaratOrg.DataAnalysis.Add(new List<DB_AGIN_Baccarat_Cell>());
                }
                for (int y_new = 0; y_new < baccaratNew.DataTotalRow; y_new++)
                {
                    if (y_new > baccaratOrg.DataAnalysis[x_org].Count - 1) // For: Allocating to row, for origin
                    {
                        baccaratOrg.DataAnalysis[x_org].Add(new DB_AGIN_Baccarat_Cell());
                    }
                    var cell_org = baccaratOrg.DataAnalysis[x_org][y_new];
                    var cell_new = baccaratNew.DataAnalysis[x_new][y_new];
                    // For: Clone infomation, for origin
                    cell_org.PercentB = cell_new.PercentB;
                    cell_org.PercentG = cell_new.PercentG;
                    cell_org.PercentR = cell_new.PercentR;
                    cell_org.Matches = cell_new.Matches;
                }
            }
        }
        
        public void SaveDB(SQLiteHelper connHelper)
        {
            string cmd = string.Format(@"INSERT INTO AGIN (Id, FileName, CoordinateX, CoordinateY, AnalysisData, CreatedOn, CreatedBy, LastModifiedOn, LastModifiedBy) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)");
            List<SQLiteParameter> paras = new List<SQLiteParameter>();
            paras.Add(new SQLiteParameter() { Value = Id });
            paras.Add(new SQLiteParameter() { Value = FileName });
            paras.Add(new SQLiteParameter() { Value = CoordinateX });
            paras.Add(new SQLiteParameter() { Value = CoordinateY });
            paras.Add(new SQLiteParameter() { Value = JsonConvert.SerializeObject(AnalysisData) });
            paras.Add(new SQLiteParameter() { Value = CreatedOn });
            paras.Add(new SQLiteParameter() { Value = CreatedBy });
            paras.Add(new SQLiteParameter() { Value = LastModifiedOn });
            paras.Add(new SQLiteParameter() { Value = LastModifiedBy });
            connHelper.ExecNonQueryCmdOptimize(paras, cmd);
        }

        public static long IdentityMax(SQLiteHelper connHelper)
        {
            long identity = 0;
            string cmd = string.Format(@"SELECT MAX(Id) FROM AGIN");
            object scalar = connHelper.ExecScalarCmd(null, cmd);
            if (DBNull.Value != scalar) { identity = (long)scalar; }
            return identity;
        }

        public static DB_AGIN_Baccarat ExtractDB(DataRow dr, List<string> cols)
        {
            DB_AGIN_Baccarat baccarat = new DB_AGIN_Baccarat();
            if (cols.Contains("Id") && DBNull.Value != dr["Id"]) { baccarat.Id = (long)dr["Id"]; }
            if (cols.Contains("FileName") && DBNull.Value != dr["FileName"]) { baccarat.FileName = (string)dr["FileName"]; }
            if (cols.Contains("CoordinateX") && DBNull.Value != dr["CoordinateX"]) { baccarat.CoordinateX = (int)dr["CoordinateX"]; }
            if (cols.Contains("CoordinateY") && DBNull.Value != dr["CoordinateY"]) { baccarat.CoordinateY = (int)dr["CoordinateY"]; }
            if (cols.Contains("AnalysisData") && DBNull.Value != dr["AnalysisData"]) { baccarat.AnalysisData = JsonConvert.DeserializeObject<DB_AGIN_Baccarat_Tbl>((string)dr["AnalysisData"]); }
            if (cols.Contains("CreatedOn") && DBNull.Value != dr["CreatedOn"]) { baccarat.CreatedOn = (DateTime)dr["CreatedOn"]; }
            if (cols.Contains("CreatedBy") && DBNull.Value != dr["CreatedBy"]) { baccarat.CreatedBy = (long)dr["CreatedBy"]; }
            if (cols.Contains("LastModifiedOn") && DBNull.Value != dr["LastModifiedOn"]) { baccarat.LastModifiedOn = (DateTime)dr["LastModifiedOn"]; }
            if (cols.Contains("LastModifiedBy") && DBNull.Value != dr["LastModifiedBy"]) { baccarat.LastModifiedBy = (long)dr["LastModifiedBy"]; }
            return baccarat;
        }

        public static DB_AGIN_Baccarat ExtractObj(int coordinateX, int coordinateY, string fileName, AGIN_3840x2160_Baccarat_TblLevel1 dataAnalysis, DateTime createdOn, long createdBy, DateTime lastModifiedOn, long lastModifiedBy)
        {
            var baccarat = new DB_AGIN_Baccarat();
            baccarat.CoordinateX = coordinateX;
            baccarat.CoordinateY = coordinateY;
            baccarat.FileNames = string.Format("{0};", fileName);
            baccarat.DataAnalysis = new List<List<DB_AGIN_Baccarat_Cell>>();
            for (int x = 0; x < dataAnalysis.Cells.Length; x++)
            {
                baccarat.DataAnalysis.Add(new List<DB_AGIN_Baccarat_Cell>());
                for (int y = 0; y < dataAnalysis.Cells[x].Length; y++)
                {
                    baccarat.DataAnalysis[x].Add(new DB_AGIN_Baccarat_Cell());
                    baccarat.DataAnalysis[x][y].PercentB = Math.Round(dataAnalysis.Cells[x][y].PercentB * 10000) / 10000;
                    baccarat.DataAnalysis[x][y].PercentG = Math.Round(dataAnalysis.Cells[x][y].PercentG * 10000) / 10000;
                    baccarat.DataAnalysis[x][y].PercentR = Math.Round(dataAnalysis.Cells[x][y].PercentR * 10000) / 10000;
                    baccarat.DataAnalysis[x][y].Matches = dataAnalysis.Cells[x][y].Matches;
                }
            }
            baccarat.CreatedOn = createdOn;
            baccarat.CreatedBy = createdBy;
            baccarat.LastModifiedOn = lastModifiedOn;
            baccarat.LastModifiedBy = lastModifiedBy;
            return baccarat;
        }
        #endregion
    }
    
    public class DB_AGIN_Baccarat_Cell
    {
        [JsonProperty("percent-b")]
        public double PercentB { get; set; }
        [JsonProperty("percent-g")]
        public double PercentG { get; set; }
        [JsonProperty("percent-r")]
        public double PercentR { get; set; }
        [JsonProperty("matches")]
        public List<string> Matches { get; set; }
        [JsonProperty("order")]
        public int Order { get; set; }
        [JsonProperty("order-confuse")]
        public bool OrderConfuse { get; set; }

        public DB_AGIN_Baccarat_Cell() { }
    }
}
