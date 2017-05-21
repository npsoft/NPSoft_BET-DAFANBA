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
        public DB_AGIN_Baccarat_Tbl DataAnalysis { get; set; }
        public DateTime CreatedOn { get; set; }
        public long CreatedBy { get; set; }
        public DateTime LastModifiedOn { get; set; }
        public long LastModifiedBy { get; set; }

        public bool AlertPattern01 { get; set; }
        public bool AlertPattern02 { get; set; }
        public bool AlertPattern03 { get; set; }
        #endregion
        #region For: Ctors
        public DB_AGIN_Baccarat()
        {
            DataAnalysis = new DB_AGIN_Baccarat_Tbl();
        }
        #endregion
        #region For: Methods
        public DB_AGIN_Baccarat Clone()
        {
            DB_AGIN_Baccarat baccarat = new DB_AGIN_Baccarat();
            baccarat.Id = Id;
            baccarat.CoordinateX = CoordinateX;
            baccarat.CoordinateY = CoordinateY;
            baccarat.FileNames = FileNames;
            baccarat.DataAnalysis = DataAnalysis.Clone();
            baccarat.CreatedOn = CreatedOn;
            baccarat.CreatedBy = CreatedBy;
            baccarat.LastModifiedOn = LastModifiedOn;
            baccarat.LastModifiedBy = LastModifiedBy;
            return baccarat;
        }
        
        /// <summary>
        /// Description: times(int), last(cell)
        /// </summary>
        public Tuple<int, DB_AGIN_Baccarat_Cell> ChkPattern01(int maxOrder = int.MaxValue)
        {
            List<DB_AGIN_Baccarat_Cell> cells = new List<DB_AGIN_Baccarat_Cell>();
            DataAnalysis.Cells.ForEach(x => {
                cells.AddRange(x.Where(y => 0 != y.Order && maxOrder >= y.Order));
            });
            cells = cells.OrderByDescending(x => x.Order).ToList();
            #region For: Calculate values
            int times = 0;
            DB_AGIN_Baccarat_Cell last = null;
            if (0 != cells.Count)
            {
                last = cells[0];
                string color = cells[0].CircleColor;
                foreach (var cell in cells)
                {
                    if (!cell.Matches.Contains(color))
                    {
                        break;
                    }
                    times++;
                }
            }
            return new Tuple<int, DB_AGIN_Baccarat_Cell>(times, last);
            #endregion
        }

        /// <summary>
        /// Description: times(int), last(cell), last-length(int), prev(cell), prev-length(int)
        /// </summary>
        public Tuple<int, DB_AGIN_Baccarat_Cell, int, DB_AGIN_Baccarat_Cell, int> ChkPattern02(int ignore, int maxOrder = int.MaxValue)
        {
            List<DB_AGIN_Baccarat_Cell> cells = new List<DB_AGIN_Baccarat_Cell>();
            DataAnalysis.Cells.ForEach(x => {
                cells.AddRange(x.Where(y => 0 != y.Order && maxOrder >= y.Order));
            });
            cells = cells.OrderByDescending(x => x.Order).ToList();
            #region For: Ignore values
            while (0 != ignore && 0 != cells.Count)
            {
                DB_AGIN_Baccarat_Cell star_cell = cells[0];
                DB_AGIN_Baccarat_Cell next_cell = null;
                string star_color = star_cell.CircleColor;
                string next_color = "";
                do
                {
                    cells.RemoveAt(0);
                    next_cell = null; next_color = "";
                    if (0 != cells.Count)
                    {
                        next_cell = cells[0];
                        next_color = next_cell.CircleColor;
                    }
                }
                while (star_color == next_color && 0 != cells.Count);
                ignore--;
            }
            #endregion
            #region For: Calculate values
            int times = 0;
            DB_AGIN_Baccarat_Cell last = null; string color_last = ""; int color_last_len = 0, color_last_len_tmp = 0;
            DB_AGIN_Baccarat_Cell prev = null; string color_prev = ""; int color_prev_len = 0, color_prev_len_tmp = 0;
            string color_curr = "", color_curr_prev = "";
            for (int i = 0; i < cells.Count; i++)
            {
                DB_AGIN_Baccarat_Cell cell = cells[i];
                color_curr = cell.CircleColor;
                if ("" == color_curr) // For: It's T case, break loop
                {
                    break;
                }
                if ("" == color_last) // For: Assign color for last, initialize color
                {
                    last = cell;
                    color_last = color_curr;
                }
                if ("" == color_prev && color_curr != color_last) // For: Assign color for previous, initialize color
                {
                    prev = cell;
                    color_prev = color_curr;
                }
                if (color_curr == color_last)
                {
                    if ("" == color_prev) // For: Assign length for last, initialize length
                    {
                        color_last_len++;
                    }
                    color_last_len_tmp++;
                }
                if (color_curr == color_prev)
                {
                    if (0 == times) // For: Assign length for previous, initialize length
                    {
                        color_prev_len++;
                    }
                    color_prev_len_tmp++;
                }
                if (color_curr != color_curr_prev)
                {
                    if ("" != color_last && "" != color_prev) // For: Don't consider last pair, initialize pair
                    {
                        if (color_curr == color_last && color_prev_len != color_prev_len_tmp ||
                            color_curr == color_prev && color_last_len != color_last_len_tmp) // For: Wrong length, break loop
                        {
                            break;
                        }
                        if (color_curr == color_last) // For: Previous pair, plus +1
                        {
                            times++;
                            color_last_len_tmp = 1;
                            color_prev_len_tmp = 0;
                        }
                    }
                    color_curr_prev = color_curr;
                }
                if (cells.Count - 1 == i && color_curr == color_prev &&
                    color_prev_len == color_prev_len_tmp && color_last_len == color_last_len_tmp) // For: Current pair, plus +1
                {
                    times++;
                }
            }
            #endregion
            return new Tuple<int, DB_AGIN_Baccarat_Cell, int, DB_AGIN_Baccarat_Cell, int>(times, last, color_last_len, prev, color_prev_len);
        }

        /// <summary>
        /// Description: color(string), last(cell), number-red(int), number-blue(int)
        /// </summary>
        public Tuple<string, DB_AGIN_Baccarat_Cell, int, int> ChkPattern03(dynamic config, int maxOrder = int.MaxValue)
        {
            List<DB_AGIN_Baccarat_Cell> cells = new List<DB_AGIN_Baccarat_Cell>();
            DataAnalysis.Cells.ForEach(x => {
                cells.AddRange(x.Where(y => 0 != y.Order && maxOrder >= y.Order));
            });
            cells = cells.OrderByDescending(x => x.Order).ToList();
            #region For: Calculate values
            string color = "";
            DB_AGIN_Baccarat_Cell last = null;
            int num_red = cells.Count(x => x.Matches.Contains("circle-red"));
            int num_blue = cells.Count(x => x.Matches.Contains("circle-blue"));
            if (0 != cells.Count)
            {
                last = cells[0];
                if (JsonHelper.GetElBySelector(config, "min-order").Value <= last.Order &&
                    JsonHelper.GetElBySelector(config, "min-circle-red").Value <= num_red &&
                    JsonHelper.GetElBySelector(config, "min-circle-blue").Value <= num_blue)
                {
                    double p_circle_rb = (double)num_red / num_blue;
                    double p_circle_br = 1 / p_circle_rb;
                    color =
                        JsonHelper.GetElBySelector(config, "p-circle-rb-min").Value > p_circle_rb || JsonHelper.GetElBySelector(config, "p-circle-br-max").Value < p_circle_br ? "circle-red" :
                        JsonHelper.GetElBySelector(config, "p-circle-rb-max").Value < p_circle_rb || JsonHelper.GetElBySelector(config, "p-circle-br-min").Value > p_circle_br ? "circle-blue" : color;
                }
            }
            return new Tuple<string, DB_AGIN_Baccarat_Cell, int, int>(color, last, num_red, num_blue);
            #endregion
        }

        public void SaveDb(SQLiteHelper connHelper)
        {
            if (0 == Id) // For: Insert data
            {
                Id = IdentityMax(connHelper) + 1;
                string cmd = string.Format(@"INSERT INTO AGIN (Id, CoordinateX, CoordinateY, FileNames, DataAnalysis, CreatedOn, CreatedBy, LastModifiedOn, LastModifiedBy) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)");
                List<SQLiteParameter> paras = new List<SQLiteParameter>();
                paras.Add(new SQLiteParameter() { Value = Id });
                paras.Add(new SQLiteParameter() { Value = CoordinateX });
                paras.Add(new SQLiteParameter() { Value = CoordinateY });
                paras.Add(new SQLiteParameter() { Value = FileNames });
                paras.Add(new SQLiteParameter() { Value = JsonConvert.SerializeObject(DataAnalysis) });
                paras.Add(new SQLiteParameter() { Value = CreatedOn });
                paras.Add(new SQLiteParameter() { Value = CreatedBy });
                paras.Add(new SQLiteParameter() { Value = LastModifiedOn });
                paras.Add(new SQLiteParameter() { Value = LastModifiedBy });
                connHelper.ExecNonQueryCmdOptimize(paras, cmd);
            }
            else // For: Update data
            {
                string cmd = string.Format(@"UPDATE AGIN SET CoordinateX = ?, CoordinateY = ?, FileNames = ?, DataAnalysis = ?, CreatedOn = ?, CreatedBy = ?, LastModifiedOn = ?, LastModifiedBy = ? WHERE Id = ?");
                List<SQLiteParameter> paras = new List<SQLiteParameter>();
                paras.Add(new SQLiteParameter() { Value = CoordinateX });
                paras.Add(new SQLiteParameter() { Value = CoordinateY });
                paras.Add(new SQLiteParameter() { Value = FileNames });
                paras.Add(new SQLiteParameter() { Value = JsonConvert.SerializeObject(DataAnalysis) });
                paras.Add(new SQLiteParameter() { Value = CreatedOn });
                paras.Add(new SQLiteParameter() { Value = CreatedBy });
                paras.Add(new SQLiteParameter() { Value = LastModifiedOn });
                paras.Add(new SQLiteParameter() { Value = LastModifiedBy });
                paras.Add(new SQLiteParameter() { Value = Id });
                connHelper.ExecNonQueryCmdOptimize(paras, cmd);
            }
        }

        public void SaveDbTrack(SQLiteHelper connHelper)
        {
            string cmd = string.Format(@"INSERT INTO AGIN_TRACK (CoordinateX, CoordinateY, FileNames, DataAnalysis, CreatedOn, CreatedBy, LastModifiedOn, LastModifiedBy) VALUES (?, ?, ?, ?, ?, ?, ?, ?)");
            List<SQLiteParameter> paras = new List<SQLiteParameter>();
            paras.Add(new SQLiteParameter() { Value = CoordinateX });
            paras.Add(new SQLiteParameter() { Value = CoordinateY });
            paras.Add(new SQLiteParameter() { Value = FileNames });
            paras.Add(new SQLiteParameter() { Value = JsonConvert.SerializeObject(DataAnalysis) });
            paras.Add(new SQLiteParameter() { Value = CreatedOn });
            paras.Add(new SQLiteParameter() { Value = CreatedBy });
            paras.Add(new SQLiteParameter() { Value = LastModifiedOn });
            paras.Add(new SQLiteParameter() { Value = LastModifiedBy });
            connHelper.ExecNonQueryCmdOptimize(paras, cmd);
        }
        
        public void SaveDbResult1(string type, int times, int latestOrder, SQLiteHelper connHelper)
        {
            string cmd = string.Format(@"INSERT INTO AGIN_RESULT1 (SubId, LatestOrder, Type, Times) VALUES (?, ?, ?, ?)");
            List<SQLiteParameter> paras = new List<SQLiteParameter>();
            paras.Add(new SQLiteParameter() { Value = Id });
            paras.Add(new SQLiteParameter() { Value = latestOrder });
            paras.Add(new SQLiteParameter() { Value = type });
            paras.Add(new SQLiteParameter() { Value = times });
            connHelper.ExecNonQueryCmdOptimize(paras, cmd);
        }

        public void SaveDbResult2(int latestOrder, int numCircleRed, int numCircleBlue, SQLiteHelper connHelper)
        {
            string cmd = string.Format(@"INSERT INTO AGIN_RESULT2 (SubId, LatestOrder, NumCircleRed, NumCircleBlue) VALUES (?, ?, ?, ?)");
            List<SQLiteParameter> paras = new List<SQLiteParameter>();
            paras.Add(new SQLiteParameter() { Value = Id });
            paras.Add(new SQLiteParameter() { Value = latestOrder });
            paras.Add(new SQLiteParameter() { Value = numCircleRed });
            paras.Add(new SQLiteParameter() { Value = numCircleBlue });
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
            if (cols.Contains("CoordinateX") && DBNull.Value != dr["CoordinateX"]) { baccarat.CoordinateX = (int)dr["CoordinateX"]; }
            if (cols.Contains("CoordinateY") && DBNull.Value != dr["CoordinateY"]) { baccarat.CoordinateY = (int)dr["CoordinateY"]; }
            if (cols.Contains("FileNames") && DBNull.Value != dr["FileNames"]) { baccarat.FileNames = (string)dr["FileNames"]; }
            if (cols.Contains("DataAnalysis") && DBNull.Value != dr["DataAnalysis"]) { baccarat.DataAnalysis = JsonConvert.DeserializeObject<DB_AGIN_Baccarat_Tbl>((string)dr["DataAnalysis"]); }
            if (cols.Contains("CreatedOn") && DBNull.Value != dr["CreatedOn"]) { baccarat.CreatedOn = (DateTime)dr["CreatedOn"]; }
            if (cols.Contains("CreatedBy") && DBNull.Value != dr["CreatedBy"]) { baccarat.CreatedBy = (long)dr["CreatedBy"]; }
            if (cols.Contains("LastModifiedOn") && DBNull.Value != dr["LastModifiedOn"]) { baccarat.LastModifiedOn = (DateTime)dr["LastModifiedOn"]; }
            if (cols.Contains("LastModifiedBy") && DBNull.Value != dr["LastModifiedBy"]) { baccarat.LastModifiedBy = (long)dr["LastModifiedBy"]; }
            return baccarat;
        }
        
        public static DB_AGIN_Baccarat ExtractObj(int coordinateX, int coordinateY, string fileName, AGIN_3840x2160_Baccarat_TblLevel1 dataAnalysis, DateTime createdOn, long createdBy, DateTime lastModifiedOn, long lastModifiedBy)
        {
            DB_AGIN_Baccarat baccarat = new DB_AGIN_Baccarat();
            baccarat.Id = 0;
            baccarat.CoordinateX = coordinateX;
            baccarat.CoordinateY = coordinateY;
            baccarat.FileNames = string.Format(";{0};", fileName);
            baccarat.DataAnalysis = new DB_AGIN_Baccarat_Tbl();
            for (int x = 0; x < dataAnalysis.Cells.Length; x++)
            {
                baccarat.DataAnalysis.Cells.Add(new List<DB_AGIN_Baccarat_Cell>());
                for (int y = 0; y < dataAnalysis.Cells[x].Length; y++)
                {
                    baccarat.DataAnalysis.Cells[x].Add(new DB_AGIN_Baccarat_Cell());
                    baccarat.DataAnalysis.Cells[x][y].CoordinateX = x;
                    baccarat.DataAnalysis.Cells[x][y].CoordinateY = y;
                    baccarat.DataAnalysis.Cells[x][y].PercentB = dataAnalysis.Cells[x][y].PercentB;
                    baccarat.DataAnalysis.Cells[x][y].PercentG = dataAnalysis.Cells[x][y].PercentG;
                    baccarat.DataAnalysis.Cells[x][y].PercentR = dataAnalysis.Cells[x][y].PercentR;
                    baccarat.DataAnalysis.Cells[x][y].Matches = dataAnalysis.Cells[x][y].Matches;
                    baccarat.DataAnalysis.Cells[x][y].CircleFsLen = dataAnalysis.Cells[x][y].CircleFsLen;
                }
            }
            baccarat.DataAnalysis.UpdTotal();
            baccarat.CreatedOn = createdOn;
            baccarat.CreatedBy = createdBy;
            baccarat.LastModifiedOn = lastModifiedOn;
            baccarat.LastModifiedBy = lastModifiedBy;
            return baccarat;
        }

        public static List<DB_AGIN_Baccarat> ExtractImg(AGIN_3840x2160_Baccarat img, string fileName, DateTime createdOn, long createdBy, DateTime lastModifiedOn, long lastModifiedBy)
        {
            List<DB_AGIN_Baccarat> agins = new List<DB_AGIN_Baccarat>();
            for (int x = 0; x < img.Tbls.Length; x++)
            {
                for (int y = 0; y < img.Tbls[0].Length; y++)
                {
                    if (null != img.Tbls[x][y])
                    {
                        agins.Add(ExtractObj(x, y, fileName, img.Tbls[x][y], createdOn, createdBy, lastModifiedOn, lastModifiedBy));
                    }
                }
            }
            return agins;
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

        public DB_AGIN_Baccarat_Tbl()
        {
            TotalCol = 0;
            TotalRow = 0;
            TotalInvalid = 0;
            LatestOrder = 0;
            LatestOrderCircle = "";
            LatestOrderX = -1;
            LatestOrderY = -1;
            LatestOrderXR = -1;
            LatestOrderYR = -1;
            Cells = new List<List<DB_AGIN_Baccarat_Cell>>();
        }
        
        public DB_AGIN_Baccarat_Tbl Clone()
        {
            DB_AGIN_Baccarat_Tbl tbl = new DB_AGIN_Baccarat_Tbl();
            tbl.TotalCol = TotalCol;
            tbl.TotalRow = TotalRow;
            tbl.TotalInvalid = TotalInvalid;
            tbl.LatestOrder = LatestOrder;
            tbl.LatestOrderCircle = LatestOrderCircle;
            tbl.LatestOrderX = LatestOrderX;
            tbl.LatestOrderY = LatestOrderY;
            tbl.LatestOrderXR = LatestOrderXR;
            tbl.LatestOrderYR = LatestOrderYR;
            Cells.ForEach(x => {
                List<DB_AGIN_Baccarat_Cell> cells = new List<DB_AGIN_Baccarat_Cell>();
                x.ForEach(y => {
                    cells.Add(y.Clone());
                });
                tbl.Cells.Add(cells);
            });
            return tbl;
        }

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
        /// Description: Update values for latest-order | latest-order-circle | latest-order-x | latest-order-y | latest-order-xr | latest-order-yr properties
        /// </summary>
        public void UpdOrder(int latestOrder, string latestOrderCircle, int latestOrderX, int latestOrderY, int latestOrderXR, int latestOrderYR)
        {
            #region For: Get some values
            int latest_order = latestOrder;
            string latest_order_circle = latestOrderCircle;
            int latest_order_x = latestOrderX;
            int latest_order_y = latestOrderY;
            int latest_order_xr = latestOrderXR;
            int latest_order_yr = latestOrderYR;
            #endregion
            for (int x = 0; x < TotalCol; x++)
            {
                #region For: If current value less than latest-order-x, then ignore it to continue with other value
                if (x < latestOrderX)
                {
                    continue;
                }
                #endregion
                for (int y = 0; y < TotalRow; y++)
                {
                    #region For: If current value less than latest-order-y, then ignore it to continue with other value
                    if (x == latestOrderX && y < latestOrderY)
                    {
                        continue;
                    }
                    #endregion
                    bool set_order = false;
                    DB_AGIN_Baccarat_Cell cell = Cells[x][y];
                    DB_AGIN_Baccarat_Cell cell_py = y - 1 > -1 ? Cells[x][y - 1] : null;
                    bool go_to_vertical = x == latestOrderX && y == latestOrderY;
                    bool go_to_horizontal = go_to_vertical && -1 != latestOrderXR && -1 != LatestOrderYR;
                    if (!go_to_vertical && 2 > cell.Matches.Count) // For: Current cell was empty, break loop
                    {
                        break;
                    }
                    if (!go_to_vertical && 0 == cell.Order && 0 == y) // For: Current cell was new, set order
                    {
                        set_order = true;
                        cell.Order = ++latest_order;
                        latest_order_circle = cell.Matches.Contains("circle-blue") ? "circle-blue" : cell.Matches.Contains("circle-red") ? "circle-red" : "";
                        #region For: Set some values
                        latest_order_x = x;
                        latest_order_y = y;
                        latest_order_xr = -1;
                        latest_order_yr = -1;
                        #endregion
                    }
                    if (!go_to_vertical && 0 == cell.Order && cell.Matches.Contains(latest_order_circle)) // For: Current cell was valid, set order
                    {
                        set_order = true;
                        cell.Order = ++latest_order;
                        latest_order_circle = cell.Matches.Contains("circle-blue") ? "circle-blue" : "circle-red";
                        #region For: Set some values
                        latest_order_x = x;
                        latest_order_y = y;
                        latest_order_xr = -1;
                        latest_order_yr = -1;
                        #endregion
                    }
                    // if (go_to_horizontal || 0 != cell.Order && !set_order || TotalRow - 1 == y && set_order) // For: Current cell was order or max index, consider right side
                    if (!go_to_vertical && (0 != cell.Order && !set_order || TotalRow - 1 == y && set_order) ||
                         go_to_vertical && (go_to_horizontal || 0 != cell.Order && null != cell_py && latestOrder == cell_py.Order || TotalRow - 1 == y && latestOrder == cell.Order)) // For: Current cell was order or max index, consider right side
                    {
                        int y_r = go_to_horizontal ? latestOrderYR :
                            !go_to_vertical && 0 != cell.Order && !set_order || go_to_vertical && 0 != cell.Order && null != cell_py && latestOrder == cell_py.Order ? y - 1 : y;
                        if (-1 != y_r) // For: Coordinate-y of right side was wrong, ignore this case
                        {
                            for (int x_r = x + 1; x_r < TotalCol; x_r++)
                            {
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
                                for (int y_c = y_r - 1; y_c > -1; y_c++)
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
                                #region For: Set some values
                                latest_order_x = x;
                                latest_order_y = y;
                                latest_order_xr = x_r;
                                latest_order_yr = y_r;
                                #endregion
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
        
        /// <summary>
        /// Description: Update values for coordinate-x | coordinate-y
        /// </summary>
        public void UpdCoordinate()
        {
            for (int col = 0; col < TotalCol; col++)
            {
                for (int row = 0; row < TotalRow; row++)
                {
                    Cells[col][row].CoordinateX = col;
                    Cells[col][row].CoordinateY = row;
                }
            }
        }

        public void DelEmpty()
        {
            for (int x = Cells.Count - 1; -1 < x; x--)
            {
                bool remove = true;
                foreach (var cell in Cells[x])
                {
                    if (1 < cell.Matches.Count)
                    {
                        remove = false;
                        break;
                    }
                }
                if (!remove) { break; }
                Cells.RemoveAt(x);
            }
            UpdTotal();
        }

        public void DelOrder(int maxOrder)
        {
            Cells.ForEach(x => {
                x.ForEach(y => {
                    y.Order = maxOrder < y.Order ? 0 : y.Order;
                });
            });
        }

        public static int DistMax(DB_AGIN_Baccarat_Tbl tblOrg, DB_AGIN_Baccarat_Tbl tblNew)
        {
            return tblNew.TotalCol / 2;
        }

        public static int DistMerge(DB_AGIN_Baccarat_Tbl tblOrg, DB_AGIN_Baccarat_Tbl tblNew, int distMax)
        {
            int dist = 0;
            bool merged = 0 == tblOrg.TotalCol; // For: Can merge with origin was empty
            for (int x_new = tblNew.TotalCol - 1; x_new > -1; x_new--)
            {
                // For: Determine distance, for new
                dist = tblNew.TotalCol - 1 - x_new;
                if (distMax < dist) // For: Distance too far, not merge
                {   
                    break;
                }
                // For: Determine min index, for origin
                int x_org_min = tblOrg.TotalCol - tblNew.TotalCol + dist;
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
            #region For: Determine start index, for both
            int x_org_sta = -1;
            for (int x = 0; x < tblOrg.TotalCol; x++)
            {
                if (0 != tblOrg.Cells[x].Count && 2 > tblOrg.Cells[x][0].Matches.Count)
                {
                    break;
                }
                x_org_sta = x;
            }
            int x_new_sta = (tblNew.TotalCol - 1 - dist) - (tblOrg.TotalCol - 1 - x_org_sta);
            if (-1 == x_org_sta) // For: Re-calculate start index, for origin
            {
                x_org_sta = 0;
            }
            if (0 > x_new_sta) // For: Re-calculate start index, for both
            {
                x_org_sta = -x_new_sta;
                x_new_sta = 0;
            }
            #endregion
            #region For: Allocating column/row, for origin
            int col_allocating = 0 != tblOrg.TotalCol ? dist : tblNew.TotalCol;
            while (0 != col_allocating)
            {
                col_allocating--;
                tblOrg.Cells.Add(new List<DB_AGIN_Baccarat_Cell>());

                int row_allocating = 0;
                while (tblNew.TotalRow != row_allocating)
                {
                    row_allocating++;
                    tblOrg.Cells[tblOrg.Cells.Count - 1].Add(new DB_AGIN_Baccarat_Cell());
                }
            }
            #endregion
            for (int x_new = x_new_sta; x_new < tblNew.TotalCol; x_new++)
            {
                for (int y_new = 0; y_new < tblNew.TotalRow; y_new++)
                {
                    var cell_new = tblNew.Cells[x_new][y_new];
                    var cell_org = tblOrg.Cells[x_org_sta + (x_new - x_new_sta)][y_new];
                    #region For: Copy infomation, for origin
                    cell_org.PercentB = cell_new.PercentB;
                    cell_org.PercentG = cell_new.PercentG;
                    cell_org.PercentR = cell_new.PercentR;
                    cell_org.Matches = cell_new.Matches;
                    #endregion
                }
            }
            tblOrg.UpdTotal();
            tblOrg.UpdCoordinate();
        }
    }

    public class DB_AGIN_Baccarat_Cell
    {
        [JsonProperty("coordinate-x")]
        public int CoordinateX { get; set; }
        [JsonProperty("coordinate-y")]
        public int CoordinateY { get; set; }
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
        [JsonProperty("circle-fs-length")]
        public int CircleFsLen { get; set; }
        public string CircleColor { get { return Matches.Contains("circle-blue") ? "circle-blue" : Matches.Contains("circle-red") ? "circle-red" : ""; } }
        public string CircleColorBR { get { return Matches.Contains("circle-blue") ? "B" : Matches.Contains("circle-red") ? "R" : ""; } }

        public DB_AGIN_Baccarat_Cell()
        {
            Matches = new List<string>();
        }

        public DB_AGIN_Baccarat_Cell Clone()
        {
            DB_AGIN_Baccarat_Cell cell = new DB_AGIN_Baccarat_Cell();
            cell.CoordinateX = CoordinateX;
            cell.CoordinateY = CoordinateY;
            cell.PercentB = PercentB;
            cell.PercentG = PercentG;
            cell.PercentR = PercentR;
            Matches.ForEach(x => { cell.Matches.Add(x); });
            cell.Order = Order;
            cell.OrderConfuse = OrderConfuse;
            cell.CircleFsLen = CircleFsLen;
            return cell;
        }
    }

    public class DB_AGIN_Baccarat_Check
    {
        #region For: Properties
        private int SubLMax { get; set; }
        private int FreqMin { get; set; }
        private KeyValuePair<int, int>[] TotalLMin { get; set; }
        private DB_AGIN_Baccarat Baccarat { get; set; }
        #endregion
        #region For: Ctors
        public DB_AGIN_Baccarat_Check(DB_AGIN_Baccarat baccarat)
        {
            Baccarat = baccarat;
            SubLMax = 9;
            FreqMin = 3;
            TotalLMin = new KeyValuePair<int, int>[9] {
                new KeyValuePair<int, int>(1, 5),
                new KeyValuePair<int, int>(2, 6),
                new KeyValuePair<int, int>(3, 9),
                new KeyValuePair<int, int>(4, 12),
                new KeyValuePair<int, int>(5, 15),
                new KeyValuePair<int, int>(6, 18),
                new KeyValuePair<int, int>(7, 21),
                new KeyValuePair<int, int>(8, 24),
                new KeyValuePair<int, int>(9, 27)};
        }

        public DB_AGIN_Baccarat_Check(DB_AGIN_Baccarat baccarat, int subLMax, int freqMin, KeyValuePair<int, int>[] totalLMin)
        {
            Baccarat = baccarat;
            SubLMax = subLMax;
            FreqMin = freqMin;
            TotalLMin = totalLMin;
        }
        #endregion
        #region For: Methods
        private bool IsDupFreq(IEnumerable<DB_AGIN_Baccarat_Cell> cellsFreq)
        {
            for (int freq_l = 1; freq_l <= cellsFreq.Count() / 2 && 0 == cellsFreq.Count() % freq_l; freq_l++)
            {
                bool is_dup = true; int freq_n = 0;
                while (is_dup && cellsFreq.Count() > ++freq_n * freq_l)
                {
                    for (int i = 0; i < freq_l; i++)
                    {
                        if (cellsFreq.ElementAt(i).CircleColor != cellsFreq.ElementAt(freq_n * freq_l + i).CircleColor)
                        {
                            is_dup = false; break;
                        }
                    }
                }
                if (is_dup) { return true; }
            }
            return false;
        }

        private bool IsValidRstItem(DB_AGIN_Baccarat_Check_RstItem rstItem)
        {
            if (FreqMin > rstItem.NFreq) { return false; }
            foreach (KeyValuePair<int, int> total_l_min in TotalLMin)
            {
                if (rstItem.CellsFreq.Count == total_l_min.Key)
                {
                    return total_l_min.Value <= rstItem.NFreq * rstItem.CellsFreq.Count + rstItem.CellsSub.Count;
                }
            }
            return true;
        }

        public List<DB_AGIN_Baccarat_Check_RstItem> Search(int maxOrder = int.MaxValue)
        {
            List<DB_AGIN_Baccarat_Check_RstItem> items = new List<DB_AGIN_Baccarat_Check_RstItem>();
            #region For: List cells and Sort by order
            List<DB_AGIN_Baccarat_Cell> cells = new List<DB_AGIN_Baccarat_Cell>();
            Baccarat.DataAnalysis.Cells.ForEach(x => {
                cells.AddRange(x.Where(y => 0 != y.Order && maxOrder >= y.Order));
            });
            cells = cells.OrderBy(x => x.Order).ToList();
            #endregion
            for (int sub_l = 0; sub_l <= SubLMax && sub_l + FreqMin * (sub_l + 1) <= cells.Count; sub_l++)
            {
                IEnumerable<DB_AGIN_Baccarat_Cell> cells_main = cells.Take(cells.Count - sub_l);
                IEnumerable<DB_AGIN_Baccarat_Cell> cells_sub = cells.Skip(cells.Count - sub_l).Take(sub_l);
                for (int freq_l = sub_l + 1; freq_l <= SubLMax + 1 && FreqMin * freq_l <= cells_main.Count(); freq_l++)
                {
                    bool stop = false; int freq_n = 0;
                    int skip = cells_main.Count() - freq_l * (freq_n + 1);
                    IEnumerable<DB_AGIN_Baccarat_Cell> cells_freq = cells_main.Skip(0 > skip ? int.MaxValue : skip).Take(freq_l);

                    if (IsDupFreq(cells_freq)) { continue; }
                    for (int i = 0; i < cells_sub.Count(); i++)
                    {
                        if (i + 1 > cells_freq.Count() || cells_sub.ElementAt(i).CircleColor != cells_freq.ElementAt(i).CircleColor)
                        {
                            stop = true; break;
                        }
                    }
                    while (!stop && 0 != ++freq_n)
                    {
                        skip = cells_main.Count() - freq_l * (freq_n + 1);
                        IEnumerable<DB_AGIN_Baccarat_Cell> cells_freqn = cells_main.Skip(0 > skip ? int.MaxValue : skip).Take(freq_l);
                        for (int i = 0; i < cells_freq.Count(); i++)
                        {
                            if (i + 1 > cells_freqn.Count() || cells_freq.ElementAt(i).CircleColor != cells_freqn.ElementAt(i).CircleColor)
                            {
                                stop = true; break;
                            }
                        }
                    }

                    DB_AGIN_Baccarat_Check_RstItem item = new DB_AGIN_Baccarat_Check_RstItem(freq_n, cells_freq.ToList(), cells_sub.ToList());
                    if (IsValidRstItem(item))
                    {
                        items.Add(item);
                    }
                }
            }
            return items;
        }
        #endregion
    }

    public class DB_AGIN_Baccarat_Check_RstItem
    {
        #region For: Properties
        public int NFreq { get; set; }
        public List<DB_AGIN_Baccarat_Cell> CellsFreq { get; set; }
        public List<DB_AGIN_Baccarat_Cell> CellsSub { get; set; }
        public string ColorsFreq { get { return string.Join("", CellsFreq.Select(x => x.CircleColorBR)); } }
        #endregion
        #region For: Ctors
        public DB_AGIN_Baccarat_Check_RstItem()
        {
            NFreq = 0;
            CellsFreq = new List<DB_AGIN_Baccarat_Cell>();
            CellsSub = new List<DB_AGIN_Baccarat_Cell>();
        }

        public DB_AGIN_Baccarat_Check_RstItem(int nFreq, List<DB_AGIN_Baccarat_Cell> cellsFreq, List<DB_AGIN_Baccarat_Cell> cellsSub)
        {
            NFreq = nFreq;
            CellsFreq = cellsFreq;
            CellsSub = cellsSub;
        }
        #endregion
    }
}
