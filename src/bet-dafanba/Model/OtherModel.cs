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
        public string FileName { get; set; }
        public int CoordinateX { get; set; }
        public int CoordinateY { get; set; }
        public DB_AGIN_Baccarat_Tbl AnalysisData { get; set; }
        public DateTime CreatedOn { get; set; }
        public long CreatedBy { get; set; }
        public DateTime LastModifiedOn { get; set; }
        public long LastModifiedBy { get; set; }
        #endregion
        #region For: Ctors
        public DB_AGIN_Baccarat() { }
        #endregion
        #region For: Methods
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

        public static DB_AGIN_Baccarat ExtractObj(long id, string fileName, int coordinateX, int coordinateY, AGIN_3840x2160_Baccarat_TblLevel1 analysisData, DateTime createdOn, long createdBy, DateTime lastModifiedOn, long lastModifiedBy)
        {
            DB_AGIN_Baccarat baccarat = new DB_AGIN_Baccarat();
            baccarat.Id = id;
            baccarat.FileName = fileName;
            baccarat.CoordinateX = coordinateX;
            baccarat.CoordinateY = coordinateY;
            baccarat.CreatedOn = createdOn;
            baccarat.CreatedBy = createdBy;
            baccarat.LastModifiedOn = lastModifiedOn;
            baccarat.LastModifiedBy = lastModifiedBy;
            baccarat.AnalysisData = new DB_AGIN_Baccarat_Tbl();
            baccarat.AnalysisData.Valid = analysisData.Valid;
            baccarat.AnalysisData.LatestCircleName = analysisData.LatestCircleName;
            baccarat.AnalysisData.LatestCircleLength = analysisData.LatestCircleLength;
            baccarat.AnalysisData.Cells = new DB_AGIN_Baccarat_Cell[analysisData.Cells.Length][];
            for (int x = 0; x < analysisData.Cells.Length; x++)
            {
                baccarat.AnalysisData.Cells[x] = new DB_AGIN_Baccarat_Cell[analysisData.Cells[x].Length];
                for (int y = 0; y < analysisData.Cells[x].Length; y++)
                {
                    baccarat.AnalysisData.Cells[x][y] = new DB_AGIN_Baccarat_Cell();
                    baccarat.AnalysisData.Cells[x][y].PercentB = Math.Round(analysisData.Cells[x][y].PercentB * 10000) / 10000;
                    baccarat.AnalysisData.Cells[x][y].PercentG = Math.Round(analysisData.Cells[x][y].PercentG * 10000) / 10000;
                    baccarat.AnalysisData.Cells[x][y].PercentR = Math.Round(analysisData.Cells[x][y].PercentR * 10000) / 10000;
                    baccarat.AnalysisData.Cells[x][y].Matches = analysisData.Cells[x][y].Matches;
                }
            }
            return baccarat;
        }
        #endregion
    }

    public class DB_AGIN_Baccarat_Tbl
    {
        [JsonProperty("valid")]
        public bool Valid { get; set; }
        [JsonProperty("latest-circle-name")]
        public string LatestCircleName { get; set; }
        [JsonProperty("latest-circle-length")]
        public int LatestCircleLength { get; set; }
        [JsonProperty("cells")]
        public DB_AGIN_Baccarat_Cell[][] Cells { get; set; }
        
        public DB_AGIN_Baccarat_Tbl() { }
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

        public DB_AGIN_Baccarat_Cell() { }
    }
}
