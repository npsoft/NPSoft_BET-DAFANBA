using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Linq;

namespace SpiralEdge.Helper
{
    public class SQLiteHelper
    {
        private string ConnString { get; set; }
        public SQLiteConnection ConnDb = new SQLiteConnection();

        public SQLiteHelper() { }

        public SQLiteHelper(string connString)
        {
            ConnString = connString;
            #region SQLiteConnection.SetPassword("{$PASSWORD}") | Not work
            /* -:
            ConnDb.ConnectionString = "Data Source=SQLiteNETADO.db3;Version=3;Pooling=True;Max Pool Size=100;";
            ConnDb.SetPassword("trungdt");
            ConnDb.Open();*/
            #endregion
            #region SQLiteConnection.ChangePassword("{$PASSWORD}")
            /* -: 
            ConnDb.ConnectionString = "Data Source=SQLiteNETADO.db3;Version=3;Pooling=True;Max Pool Size=100;";
            ConnDb.Open();
            ConnDb.ChangePassword("trungdt");*/
            #endregion
            #region SQLiteConnection.ChangePassword(string.Empty)
            /* -: ConnDb.ConnectionString = "Data Source=SQLiteNETADO.db3;Version=3;Password=trungdt;Pooling=True;Max Pool Size=100;";
            ConnDb.Open();
            ConnDb.ChangePassword(string.Empty);*/
            #endregion
            /* -: DbProviderFactory fact = DbProviderFactories.GetFactory("System.Data.SQLite");
            using (DbConnection cnn = fact.CreateConnection())
            {
                cnn.ConnectionString = "Data Source = test.db3";
                cnn.Open();
            }*/
            /* -: ConnDb.ConnectionString = ConnString;
            ConnDb.Open();
            ConnDb.ChangePassword("trungdt");*/
        }

        public bool Open()
        {
            if (ConnDb.State == ConnectionState.Closed)
            {
                ConnDb.ConnectionString = ConnString;
                try
                {
                    ConnDb.Open();
                }
                catch (Exception ex)
                {
                    // -: System.Diagnostics.Debug.Print(string.Format("Exception\t:: {0}{1}", ex.Message, ex.StackTrace));
                    throw new Exception(string.Format("Exception\t:: {0}{1}", ex.Message, ex.StackTrace), ex);
                    return false;
                }
            }
            return true;
        }

        public bool Close()
        {
            if (ConnDb.State == ConnectionState.Open)
            {
                try
                {
                    ConnDb.Close();
                }
                catch (Exception ex)
                {
                    // -: System.Diagnostics.Debug.Print(string.Format("Exception\t:: {0}{1}", ex.Message, ex.StackTrace));
                    throw new Exception(string.Format("Exception\t:: {0}{1}", ex.Message, ex.StackTrace), ex);
                    return false;
                }
            }
            return true;
        }

        public DataSet ExecCmd(SQLiteCommand cmd)
        {
            DataSet ds = new DataSet();
            SQLiteDataAdapter da = new SQLiteDataAdapter(cmd);
            cmd.Connection = ConnDb;
            Open();
            da.Fill(ds);
            Close();
            return ds;
        }

        public object ExecScalarCmd(List<SQLiteParameter> lstPara, string cmdText)
        {
            object scalar = null;
            using (SQLiteCommand cmd = new SQLiteCommand(ConnDb))
            {
                Open();
                cmd.CommandText = cmdText;
                foreach (SQLiteParameter para in lstPara ?? Enumerable.Empty<SQLiteParameter>())
                {
                    cmd.Parameters.Add(para);
                }
                scalar = cmd.ExecuteScalar();
                Close();
            }
            return scalar;
        }

        public void ExecNonQueryCmd(List<SQLiteParameter> lstPara, string cmdText)
        {
            using (SQLiteCommand cmd = new SQLiteCommand(ConnDb))
            {
                Open();
                cmd.CommandText = cmdText;
                foreach (SQLiteParameter para in lstPara ?? Enumerable.Empty<SQLiteParameter>())
                {
                    cmd.Parameters.Add(para);
                }
                cmd.ExecuteNonQuery();
                Close();
            }
        }

        public void ExecNonQueryCmdOptimize(List<SQLiteParameter> lstPara, string cmdText)
        {
            Open();
            using (SQLiteTransaction transaction = ConnDb.BeginTransaction())
            {
                using (SQLiteCommand cmd = new SQLiteCommand(ConnDb))
                {
                    cmd.CommandText = cmdText;
                    foreach (SQLiteParameter para in lstPara ?? Enumerable.Empty<SQLiteParameter>())
                    {
                        cmd.Parameters.Add(para);
                    }
                    cmd.ExecuteNonQuery();
                }
                transaction.Commit();
            }
            Close();
        }
        
        public void SampleExecCmd()
        {
            SQLiteCommand cmd = ConnDb.CreateCommand();
            try
            {
                #region SQLiteCommand: Initialize
                #region cmd.CommandText = string.Format(@"")
                cmd.CommandText = string.Format("SELECT [MyId] FROM [MyTable] LIMIT ?, ?");
                #endregion
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = 2400;
                #endregion
                #region SQLiteCommand: Parameters
                cmd.Parameters.Add(new SQLiteParameter() { Value = 1 });
                cmd.Parameters.Add(new SQLiteParameter() { Value = 10 });
                #endregion
                #region SQLiteCommand: Connection
                DataSet ds = ExecCmd(cmd);
                #endregion
                #region For: Retrieve
                DataTable dt = ds.Tables[0];
                foreach (DataRow dr in dt.Rows)
                {
                    System.Diagnostics.Debug.Print(string.Format("Information\t:: [ id = {0} ] ", dr["MyId"]));
                }
                #endregion
                #region For: Clean data
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
    }
}
