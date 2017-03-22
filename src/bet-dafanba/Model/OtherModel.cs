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
    
    public class DB_AGIN_Baccarat_Tbl
    {
        [JsonProperty("total-col")]
        public int TotalCol { get; set; }
        [JsonProperty("total-row")]
        public int TotalRow { get; set; }
        [JsonProperty("total-invalid")]
        public int TotalInvalid { get; set; }
        [JsonProperty("latest-order")]
        public int LatestOrder { get; set; }
        [JsonProperty("latest-order-circle")]
        public string LatestOrderCircle { get; set; }
        [JsonProperty("latest-order-x")]
        public int LatestOrderX { get; set; }
        [JsonProperty("latest-order-y")]
        public int LatestOrderY { get; set; }
        [JsonProperty("latest-order-xr")]
        public int LatestOrderXR { get; set; }
        [JsonProperty("latest-order-yr")]
        public int LatestOrderYR { get; set; }
        [JsonProperty("cells")]
        public List<List<DB_AGIN_Baccarat_Cell>> Cells { get; set; }

        public DB_AGIN_Baccarat_Tbl() { }
        
        /// <summary>
        /// Description: Update values for total-col | total-row | total-invalid properties
        /// </summary>
        public void UpdTotal()
        {
            #region For: total-col
            TotalCol = Cells.Count;
            #endregion
            #region For: total-row
            TotalRow = 0 != TotalCol ? Cells[0].Count : 0;
            #endregion
            #region For: total-invalid
            TotalInvalid = 0;
            for (int col = 0; col < TotalCol; col++)
            {
                for (int row = 0; row < TotalRow; row++)
                {
                    if (0 == Cells[col][row].Matches.Count)
                    {
                        TotalInvalid++;
                    }
                }
            }
            #endregion
        }

        /// <summary>
        /// Description: Default values for latest-order = 0 | latest-order-circle = empty | latest-order-x = -1 | latest-order-y = -1 | latest-order-xr = -1 | latest-order-yr = -1
        /// </summary>
        public void UpdOrder(int latestOrder, string latestOrderCircle, int latestOrderX, int latestOrderY, int latestOrderXR, int latestOrderYR)
        {
            #region For: Get some values
            int latest_order = latestOrder;
            string latest_order_circle = latestOrderCircle;
            int latest_order_x = -1;
            int latest_order_y = -1;
            int latest_order_xr = -1;
            int latest_order_yr = -1;
            #endregion
            for (int x = 0; x < TotalCol; x++)
            {
                latest_order_x = x;
                #region For: If current value less than latest-order-x, then ignore it to continue with other value
                if (x < latestOrderX)
                {
                    break;
                }
                #endregion
                for (int y = 0; y < TotalRow; y++)
                {
                    latest_order_y = y;
                    latest_order_xr = -1;
                    latest_order_yr = -1;
                    #region For: If current value less than latest-order-y, then ignore it to continue with other value
                    if (y < latestOrderY)
                    {
                        break;
                    }
                    #endregion
                    bool set_order = false;
                    DB_AGIN_Baccarat_Cell cell = Cells[x][y];
                    bool go_to_horizontal = x == latestOrderX && y == latestOrderY && -1 != latestOrderXR && -1 != LatestOrderYR;
                    if (!go_to_horizontal && 2 > cell.Matches.Count) // For: Current cell was empty, break loop
                    {
                        break;
                    }
                    if (!go_to_horizontal && 0 == cell.Order && 0 == y) // For: Current cell was new, set order
                    {
                        set_order = true;
                        cell.Order = ++latest_order;
                        latest_order_circle = cell.Matches.Contains("circle-blue") ? "circle-blue" : cell.Matches.Contains("circle-red") ? "circle-red" : "";
                    }
                    if (!go_to_horizontal && 0 == cell.Order && cell.Matches.Contains(latest_order_circle)) // For: Current cell was valid, set order
                    {
                        set_order = true;
                        cell.Order = ++latest_order;
                        latest_order_circle = cell.Matches.Contains("circle-blue") ? "circle-blue" : "circle-red";
                    }
                    if (go_to_horizontal || 0 != cell.Order && !set_order || TotalRow - 1 == y && set_order) // For: Current cell was order or max index, consider right side
                    {
                        var y_r = go_to_horizontal ? latestOrderYR : 0 != cell.Order && !set_order ? y - 1 : y;
                        if (-1 != y_r) // For: Coordinate-y of right side was wrong, ignore this case
                        {
                            latest_order_yr = y_r;
                            for (var x_r = x + 1; x_r < TotalCol; x_r++)
                            {
                                latest_order_xr = x_r;
                                #region For: If current value less than or equal latest-order-xr, then ignore it to continue with other value
                                if (x_r <= latestOrderXR)
                                {
                                    continue;
                                }
                                #endregion
                                var cell_r = Cells[x_r][y_r];
                                if (!cell_r.Matches.Contains(latest_order_circle)) // For: Right cell was invalid, break loop
                                {
                                    break;
                                }
                                var order_confuse = true; // For: Order can confuse, not sure about this check
                                for (var y_c = y_r - 1; y_c > -1; y_c++)
                                {
                                    var cell_c = Cells[x_r][y_c];
                                    if (!cell_c.Matches.Contains(latest_order_circle) || x_r < TotalCol && Cells[x_r + 1][y_c].Matches.Contains(latest_order_circle))
                                    {
                                        order_confuse = false;
                                        break;
                                    }
                                }
                                cell_r.Order = ++latest_order;
                                cell_r.OrderConfuse = order_confuse;
                            }
                        }
                        break;
                    }
                }
            }
            #region For: Set some values
            LatestOrder = latest_order;
            LatestOrderCircle = latest_order_circle;
            LatestOrderX = latest_order_x;
            LatestOrderY = latest_order_y;
            LatestOrderXR = latest_order_xr;
            LatestOrderYR = latest_order_yr;
            #endregion
        }
        
        public static int CheckMerge(DB_AGIN_Baccarat_Tbl tblOrg, DB_AGIN_Baccarat_Tbl tblNew, int distMax)
        {
            bool merged = false; int dist = 0;
            if (0 == tblNew.TotalCol && 0 == tblOrg.TotalCol)
            {
                // can merge
            }
            for (int x_new = tblNew.TotalCol - 1; x_new > -1; x_new--)
            {
                // For: Determine distance, for new
                dist = tblNew.TotalCol - 1 - x_new;
                if (distMax < dist) // For: Distance too far, not merge
                {
                    break;
                }
                // For: Determine min index, for origin
                int x_org_min = tblOrg.TotalCol - tblNew.TotalCol - dist;
                if (0 > x_org_min) // For: Columns not enough, ignore it
                {
                    continue;
                }
                // For: Check cells were order, for origin
                bool flag = true;
                for (int x_org = tblOrg.TotalCol - 1; x_org > x_org_min - 1; x_org--)
                {
                    for (int y_org = tblOrg.TotalRow - 1; y_org > -1; y_org--)
                    {
                        DB_AGIN_Baccarat_Cell cell_org = tblOrg.Cells[x_org][y_org];
                        DB_AGIN_Baccarat_Cell cell_new = tblNew.Cells[x_org - x_org_min][y_org];
                        string color_org = cell_org.Matches.Contains("circle-blue") ? "circle-blue" : cell_org.Matches.Contains("circle-red") ? "circle-red" : "";
                        string color_new = cell_new.Matches.Contains("circle-blue") ? "circle-blue" : cell_new.Matches.Contains("circle-red") ? "circle-red" : "";
                        if (0 != cell_org.Order && color_org != color_new) // For: Cell was order but different, ignore it
                        {
                            flag = false;
                            break;
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

        public static void ExecMerge(DB_AGIN_Baccarat_Tbl tblOrg, DB_AGIN_Baccarat_Tbl tblNew, int dist)
        {
            #region For: Determine max index, for origin
            int x_org_empty = 0;
            for (int x = 0; x < tblOrg.TotalCol; x++)
            {
                if (0 != tblOrg.Cells[x].Count && 2 > tblOrg.Cells[x][0].Matches.Count)
                {
                    break;
                }
                x_org_empty++;
            }
            #endregion
            #region For: Allocating column/row, for origin
            int col_allocating = 0;
            while (dist != col_allocating)
            {
                col_allocating++;
                tblOrg.Cells.Add(new List<DB_AGIN_Baccarat_Cell>());

                int row_allocating = 0;
                while (tblNew.TotalRow != row_allocating)
                {
                    row_allocating++;
                    tblOrg.Cells[tblOrg.Cells.Count - 1].Add(new DB_AGIN_Baccarat_Cell());
                }
            }
            #endregion
            for (int x_org = x_org_empty; x_org < tblOrg.TotalCol + dist; x_org++)
            {
                for (int y_org = 0; y_org < tblOrg.TotalRow; y_org++)
                {
                    var cell_org = tblOrg.Cells[x_org][y_org];
                    var cell_new = tblNew.Cells[x_org - x_org_empty (tblNew.TotalCol - dist)][y_org];
                    #region For: Copy infomation, for origin
                    cell_org.PercentB = cell_new.PercentB;
                    cell_org.PercentG = cell_new.PercentG;
                    cell_org.PercentR = cell_new.PercentR;
                    cell_org.Matches = cell_new.Matches;
                    #endregion
                }
            }
        }
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
