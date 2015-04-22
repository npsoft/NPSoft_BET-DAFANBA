using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Data;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Ionic.Zip;
using ABSoft.Photobookmart.CleanOldPhotobook.Components;
using System.Configuration;
using ServiceStack.OrmLite;
using ServiceStack.OrmLite.SqlServer;
using PhotoBookmart.Common.ServiceStackHelper.ORM;
using PhotoBookmart.DataLayer.Models.Products;
using PhotoBookmart.DataLayer.Models.System;

namespace ABSoft.Photobookmart.CleanOldPhotobook.ServiceInterface
{
    /// <summary>
    /// Internal Photobook Clean Service
    /// </summary>
    public class PhotobookmartService
    {
        #region Variables
        Thread _thread, _thread_digilabs_watcher;
        IRBLog log;

        int cleanDuration = 2;
        string clean_path = "";
        OrmLiteConnectionFactory dbFactory;
        #endregion

        #region Properties
        IDbConnection _connection = null;
        IDbConnection Db
        {
            get
            {
                if (_connection == null || _connection.State != ConnectionState.Open)
                {
                    _connection = dbFactory.Open();
                }
                return _connection;
            }
        }

        /// <summary>
        /// Path of Input decryption folder, get from settings
        /// </summary>
        string PATH_DECRYPTION_INPUT { get; set; }

        /// <summary>
        /// Path of Output decryption folder, get from settings
        /// </summary>
        string PATH_DECRYPTION_OUTPUT { get; set; }

        #endregion

        public PhotobookmartService(IRBLog log)
        {
            this.log = log;
        }

        #region Files & Folder
        /// <summary>
        /// Return true if folder is in format XXXXXX with X is digit
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        bool OrderFolderNameValidate(string name)
        {
            string strRegex = @"^\d{6,}";
            Regex myRegex = new Regex(strRegex, RegexOptions.None);

            if (myRegex.Matches(name).Count == 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Combine and prepare the path
        /// </summary>
        /// <param name="path"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        string File_CombinePath(string path, string name, bool isWindows = true)
        {
            string endsuffix = "\\";
            if (!isWindows)
            {
                endsuffix = "/";
            }
            if (!path.EndsWith(endsuffix))
            {
                path += endsuffix;
            }
            return path + name;
        }

        /// <summary>
        /// creeate the directory and return the directory information
        /// </summary>
        /// <param name="path"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        DirectoryInfo File_CreateDirectory(string path, string name)
        {
            path = File_CombinePath(path, name);
            if (!Directory.Exists(path))
            {
                return Directory.CreateDirectory(path);
            }
            else
            {
                return new DirectoryInfo(path);
            }
        }
        #endregion

        #region Database
        /// <summary>
        /// Return True if can init db connection
        /// </summary>
        /// <returns></returns>
        bool Init_DbConnection()
        {
            var cs = ConfigurationManager.AppSettings.Get("ConnectionString");

            // fix the bug Dialect Postgresql provider wrong datetime
            SqlServerOrmLiteDialectProvider dialect = SqlServerOrmLiteDialectProvider.Instance;
            dialect.UseUnicode = true;
            dialect.UseDatetime2(true);
            dialect.StringColumnDefinition = "nvarchar(MAX)";
            dialect.StringLengthColumnDefinitionFormat = dialect.StringColumnDefinition;
            dialect.StringLengthNonUnicodeColumnDefinitionFormat = dialect.StringColumnDefinition;
            dialect.StringLengthUnicodeColumnDefinitionFormat = dialect.StringColumnDefinition;
            dialect.NamingStrategy = new ABNamingStrategy();
            dbFactory = new OrmLiteConnectionFactory(cs, dialect);
            dbFactory.AutoDisposeConnection = true;

            // now we try to open a connection to check 
            try
            {
                PATH_DECRYPTION_INPUT = Db_GetSetting(Enum_Settings_Key.WEBSITE_DGL_AUTODECRYPT_INPUT, System.IO.Path.GetTempPath(), Enum_Settings_DataType.String).ToString();
                PATH_DECRYPTION_OUTPUT = Db_GetSetting(Enum_Settings_Key.WEBSITE_DGL_AUTODECRYPT_OUTPUT, System.IO.Path.GetTempPath(), Enum_Settings_DataType.String).ToString();

                log.Log(string.Format("Auto Decryption: Input folder = {0}, Output = {1}", PATH_DECRYPTION_INPUT, PATH_DECRYPTION_OUTPUT));
            }
            catch (Exception ex)
            {
                log.Log("Auto Decryption");
                log.Log(ex);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Get settings from Database. Use this function instead of the one in Settings class to avoid the confiction
        /// </summary>
        /// <param name="key"></param>
        /// <param name="default_value"></param>
        /// <param name="data_type"></param>
        /// <returns></returns>
        object Db_GetSetting(Enum_Settings_Key ekey, object default_value = null, Enum_Settings_DataType data_type = Enum_Settings_DataType.Raw)
        {

            string value = "";
            string key = ekey.ToString();

            try
            {
                var k = Db.Select<Settings>(x => x.Where(m => m.Key == key).Limit(1)).FirstOrDefault();
                if (k == null)
                {
                    value = default_value.ToString();
                }
                else
                {
                    value = k.Value;
                }

                // // we dispose the connection to save resource
                Db.Close();

                switch (data_type)
                {
                    case Enum_Settings_DataType.Int:
                        return int.Parse(value.ToString());

                    case Enum_Settings_DataType.Double:
                        return double.Parse(value.ToString());

                    case Enum_Settings_DataType.Percent:
                        value = value.Substring(0, value.Length - 1);
                        return double.Parse(value);

                    case Enum_Settings_DataType.String:
                        return value.ToString();

                    case Enum_Settings_DataType.DateTime:
                        return DateTime.Parse(value.ToString());

                    case Enum_Settings_DataType.Raw:
                        return k;
                    default:
                        return value;
                }

            }
            catch (Exception ex)
            {
                return default_value;
            }
        }

        public void AddHistory(long orderId, string content, string name, int user_id, bool isPrivate = false)
        {
            Order_History h = new Order_History();
            h.Content = content;
            h.OnDate = DateTime.Now;
            h.Order_Id = orderId;
            h.UserName = name;
            h.UserId = user_id;
            h.isPrivate = isPrivate;
            Db.Insert<Order_History>(h);
            Db.Close();
        }

        #endregion

        #region Threading Functions
        void Sleep()
        {
            Thread.Sleep(0);
            Thread.Sleep(200);
            Thread.Sleep(0);
        }

        /// <summary>
        /// Start running the service
        /// </summary>
        public void Start()
        {
            #region Thread to clean old Photobook Code folder
            this.cleanDuration = int.Parse(ConfigurationManager.AppSettings.Get("Timeoutday"));
            this.clean_path = ConfigurationManager.AppSettings.Get("Path");

            // check local directory
            if (!clean_path.EndsWith("\\"))
            {
                clean_path += "\\";
            }

            if (!Directory.Exists(clean_path))
            {
                log.Log("Folder " + clean_path + " is not existing");
            }
            else
            {
                // start the thread to wait for each minutes
                _thread = new Thread(new ThreadStart(SyncThreading));
                _thread.Priority = ThreadPriority.AboveNormal;
                 _thread.Start();
            }
            #endregion

            #region Thread for Auto Decryption

            if (Init_DbConnection())
            {
                _thread_digilabs_watcher = new Thread(new ThreadStart(Threading_Digilabs_Server_Watcher));
                _thread_digilabs_watcher.Priority = ThreadPriority.AboveNormal;
                _thread_digilabs_watcher.Start();
            }

            #endregion
        }

        /// <summary>
        /// Stop the service 
        /// </summary>
        public void Stop()
        {
            try
            {
                if (_connection != null)
                {
                    _connection.Close();
                }
                _thread.Abort();
                _thread_digilabs_watcher.Abort();
            }
            catch
            {
            }
        }

        /// <summary>
        /// Function to run by thread
        /// </summary>
        bool IsRunningSync = false;
        void SyncThreading()
        {
            List<string> blacklist_foldername = new List<string>();
            blacklist_foldername.Add("System Volume Information");
            blacklist_foldername.Add(@"$RECYCLE.BIN");
            blacklist_foldername.Add(".");
            blacklist_foldername.Add("..");

            while (_thread.IsAlive)
            {
                try
                {
                    // validate mins first to be sure we are at the time to process
                    if (!IsRunningSync)
                    {
                        lock (_thread)
                        {
                            IsRunningSync = true;
                        }
                        log.Log("Thread has been started ");

                        // check the folder
                        if (!Directory.Exists(clean_path))
                        {
                            log.Log("Folder " + clean_path + " is not existing");
                            IsRunningSync = false;
                            break;
                        }

                        //
                        DirectoryInfo dir = new DirectoryInfo(clean_path);

                        // check server to sync
                        foreach (var dir1 in dir.GetDirectories())
                        {
                            // in blacklist?
                            if (blacklist_foldername.Where(x => x.ToLower() == dir1.Name.ToLower()).Count() > 0)
                            {
                                continue;
                            }

                            // finish this directory? check to delete if expiration or delete on success
                            if (DateTime.Now.Subtract(TimeSpan.FromHours(cleanDuration)) > dir1.LastWriteTime)
                            {
                                // delete
                                try
                                {
                                    log.Log("Delete directory " + dir1.FullName);
                                    dir1.Delete(true);
                                }
                                catch (Exception ex)
                                {
                                    log.Log("Delete directory " + dir1.FullName + " not success.");
                                    log.Log(ex);
                                }
                                continue;
                            }
                        }

                        // done? then set no Sync
                        IsRunningSync = false;
                        log.Log("Thread has been finished");
                    }
                }
                catch (Exception ex)
                {
                    log.Log(ex);
                    IsRunningSync = false;
                }
                Thread.Sleep(1000 * 60 * 5); // sleep in one minute
            }
        }

        bool IsRunning_Digilabs_Watcher = false;

        void Threading_Digilabs_Server_Watcher()
        {
            while (_thread_digilabs_watcher.IsAlive)
            {
                try
                {
                    if (!IsRunning_Digilabs_Watcher)
                    {
                        lock (_thread_digilabs_watcher)
                        {
                            IsRunning_Digilabs_Watcher = true;
                        }

                        log.Log("Digilabs Watcher thread has been started");

                        var orderUploadTickets = Db.Select<Order_UploadFilesTicket>(x => x.Where(y => (
                            (y.Status == (int)Enum_UploadFilesTicketStatus.Uploaded) || (y.Status == (int)Enum_UploadFilesTicketStatus.MoveToDataFolder))));

                        string pathCustomerUpload = Db_GetSetting(Enum_Settings_Key.WEBSITE_CUSTOMER_UPLOAD_PATH_DEFAULT, System.IO.Path.GetTempPath(), Enum_Settings_DataType.String).ToString();

                        string pathDGLAutoDecryptInput = Db_GetSetting(Enum_Settings_Key.WEBSITE_DGL_AUTODECRYPT_INPUT, System.IO.Path.GetTempPath(), Enum_Settings_DataType.String).ToString();

                        string pathDGLAutoDecryptOutput = Db_GetSetting(Enum_Settings_Key.WEBSITE_DGL_AUTODECRYPT_OUTPUT, System.IO.Path.GetTempPath(), Enum_Settings_DataType.String).ToString();

                        // pathCustomerUpload = @"J:\GSOFT-gLogistics";

                        // pathDGLAutoDecryptInput = @"J:\GSOFT-gLogistics\gLogistics\DATABASE";

                        // pathDGLAutoDecryptOutput = @"J:\GSOFT-gLogistics\gLogistics\REFERENCE";

                        foreach (var orderUploadTicket in orderUploadTickets)
                        {
                            var order = Db.Select<Order>(x => x.Where(y => (y.Id == orderUploadTicket.OrderId)).Limit(1)).First();

                            if (order == null)
                            {
                                continue;
                            }

                            try
                            {
                                string pathDirZip = string.Format("{0}\\{1}_{2}", pathDGLAutoDecryptOutput, order.Id, orderUploadTicket.Id);

                                string pathFileZip = string.Format("{0}\\{1}_{2}.zip", pathCustomerUpload, order.Id, orderUploadTicket.Id);


                                if (orderUploadTicket.Status == (int)Enum_UploadFilesTicketStatus.Uploaded)
                                {
                                    string pathCustomerUploadFile = string.Format("{0}\\{1}_{2}.dgl", pathCustomerUpload, order.Id, orderUploadTicket.Id);

                                    string pathDGLAutoDecryptInputFile = string.Format("{0}\\{1}_{2}.dgl", pathDGLAutoDecryptInput, order.Id, orderUploadTicket.Id);

                                    string pathDGLAutoDecryptOutputFile = string.Format("{0}\\done\\{1}_{2}.dgl", pathDGLAutoDecryptOutput, order.Id, orderUploadTicket.Id);

                                   
                                    #region 1. Copy File To Input

                                    log.Log(string.Format("Starting - Copy File: {0} to {1}", pathCustomerUploadFile, pathDGLAutoDecryptInputFile));

                                    //CreateFileFromPath(pathDGLAutoDecryptInputFile, pathCustomerUploadFile);
                                    System.IO.File.Copy(pathCustomerUploadFile, pathDGLAutoDecryptInputFile);

                                    log.Log(string.Format("Completed - Copy File: {0} to {1}", pathCustomerUploadFile, pathDGLAutoDecryptInputFile));

                                    #endregion

                                    #region 2. DGL Auto Decrypt File

                                    log.Log(string.Format("Waiting - DGL Auto Decrypt File: {0}", pathDGLAutoDecryptInputFile));

                                    while (File.Exists(pathDGLAutoDecryptInputFile)) Thread.Sleep(1000 * 15);

                                    log.Log(string.Format("Completed - DGL Auto Decrypt File: {0}", pathDGLAutoDecryptInputFile));

                                    #endregion

                                    #region 3. Zip Folder After Decrypt

                                    log.Log(string.Format("Waiting - Zip Folder: {0}", pathDirZip));

                                    ZipFolderDGLFromPath(pathDirZip, pathFileZip);

                                    log.Log(string.Format("Completed - Zip Folder: {0}", pathFileZip));

                                    #endregion

                                    #region 5. Delete Files & Folder

                                    //DeleteFolderFromPath(pathDirZip);

                                    log.Log(string.Format("Deleted Folder: {0}", pathDirZip));

                                    File.Delete(pathDGLAutoDecryptOutputFile);

                                    log.Log(string.Format("Deleted File: {0}", pathDGLAutoDecryptOutputFile));

                                    File.Delete(pathCustomerUploadFile);

                                    log.Log(string.Format("Deleted File: {0}", pathCustomerUploadFile));

                                    // after this step we keep the data file in 2 places, one in the zip file and one is the data folder

                                    #endregion

                                    #region 6. Update Ticket & Order

                                    orderUploadTicket.Status = (int)Enum_UploadFilesTicketStatus.DecryptedSuccess;

                                    orderUploadTicket.LastUpdate = DateTime.Now;

                                    Db.UpdateOnly<Order_UploadFilesTicket>(orderUploadTicket, ev => ev.Update(p => new
                                    {
                                        p.Status,
                                        p.LastUpdate,
                                    }).Where(m => (m.Id == orderUploadTicket.Id)).Limit(1));

                                    order.LastUpdate = DateTime.Now;

                                    Db.UpdateOnly<Order>(order, ev => ev.Update(p => new
                                    {
                                        p.LastUpdate
                                    }).Where(m => (m.Id == order.Id)).Limit(1));

                                    AddHistory(order.Id, "System decrypted the uploaded file from customer.", "System", 0, false);

                                    #endregion
                                }
                                else if (orderUploadTicket.Status == (int)Enum_UploadFilesTicketStatus.MoveToDataFolder)
                                {
                                    // move data folder
                                    string pathWebsite = Db_GetSetting(Enum_Settings_Key.WEBSITE_UPLOAD_PATH_DEFAULT, System.IO.Path.GetTempPath(), Enum_Settings_DataType.String).ToString();

                                    if (!order.Payment_isPaid)
                                    {
                                        // select difference folder, the not paid folder

                                        pathWebsite = Db_GetSetting(Enum_Settings_Key.WEBSITE_ORDERS_FOLDER_NOTYETPAID_PATH, System.IO.Path.GetTempPath(), Enum_Settings_DataType.String).ToString();
                                    }

                                    string destPath = string.Format(@"{0}\{1}", pathWebsite, order.Order_Number);

                                    if (string.IsNullOrEmpty(pathWebsite) || !Directory.Exists(string.Format(@"{0}\{1}", pathWebsite, order.Order_Number)))
                                    {
                                        try
                                        {
                                            Directory.CreateDirectory(string.Format(@"{0}\{1}", pathWebsite, order.Order_Number));
                                        }
                                        catch (Exception ex)
                                        {
                                            throw new Exception("Can not create folder " + destPath, ex);
                                            //continue;
                                        }
                                    }

                                    try
                                    {
                                        var d = new DirectoryInfo(pathDirZip);
                                        var list_files = d.GetFiles();
                                        foreach (var f in list_files)
                                        {
                                            File.Copy(f.FullName, destPath + "\\" + f.Name, true);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        throw new Exception("Can not move file to Data Folder. Exception: " + ex.Message, ex);
                                        //continue;
                                    }

                                    try
                                    {
                                        var d = new DirectoryInfo(pathDirZip);
                                        var list_files = d.GetFiles();
                                        foreach (var f in list_files)
                                        {
                                            File.Delete(f.FullName);
                                        }
                                    }
                                    catch
                                    {
                                    }
                                    try
                                    {
                                        System.IO.File.Delete(pathFileZip);
                                    }
                                    catch
                                    {

                                    }

                                    AddHistory(order.Id, "New file has been replaced old data file. File upload request ticket closed.", "System", 0);

                                    Db.UpdateOnly<Order>(order, ev => ev.Update(p => new
                                    {
                                        p.LastUpdate
                                    }).Where(m => (m.Id == order.Id)).Limit(1));
                                    Db.Delete<Order_UploadFilesTicket>(x => x.Where(y => (y.Id == orderUploadTicket.Id)).Limit(1));
                                }
                            }
                            catch (Exception ex)
                            {
                                orderUploadTicket.Status = (int)Enum_UploadFilesTicketStatus.DecryptedFailed;

                                orderUploadTicket.LastUpdate = DateTime.Now;

                                Db.UpdateOnly<Order_UploadFilesTicket>(orderUploadTicket, ev => ev.Update(p => new
                                {
                                    p.Status,
                                }).Where(m => (m.Id == orderUploadTicket.Id)).Limit(1));

                                Db.UpdateOnly<Order>(new Order { LastUpdate = DateTime.Now }, ev => ev.Update(p => new
                                {
                                    p.LastUpdate,
                                }).Where(m => (m.Id == orderUploadTicket.OrderId)).Limit(1));

                                AddHistory(order.Id, string.Format("System got exception while decrypting the file from customer. Exception: [{0}]", ex.Message), "System", 0, false);
                            }
                        }

                        IsRunning_Digilabs_Watcher = false;

                        log.Log("Digilab watcher thread has been finished");
                    }
                }
                catch (Exception ex)
                {
                    log.Log(ex);

                    IsRunning_Digilabs_Watcher = false;
                }

                Thread.Sleep(1000 * 60 * 3);
            }
        }

        // Zip Folder DGL
        public void ZipFolderDGLFromPath(string orgPath, string desPath)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(orgPath);

            using (ZipFile zip = new ZipFile())
            {
                zip.AlternateEncodingUsage = ZipOption.AsNecessary;

                foreach (FileInfo file in dirInfo.GetFiles())
                {
                    zip.AddEntry(file.Name, File.ReadAllBytes(file.FullName));
                }

                Stream streamZip = new MemoryStream();

                zip.Save(streamZip);

                CreateFileFromStream(desPath, streamZip);
            }
        }

        // Delete Folder From Path
        public void DeleteFolderFromPath(string orgPath)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(orgPath);

            List<string> lstPathFile = new List<string>();

            foreach (FileInfo file in dirInfo.GetFiles())
            {
                lstPathFile.Add(file.FullName);
            }

            foreach (string pathFile in lstPathFile)
            {
                File.Delete(pathFile);
            }

            Directory.Delete(orgPath);
        }

        //// Create File From Path
        //public void CreateFileFromPath(string desPath, string orgPath)
        //{
        //    int bytesToRead = 1024 * 16;

        //    byte[] buffer = new Byte[bytesToRead];

        //    var reader = File.OpenRead(orgPath);

        //    var writer = File.Create(desPath, bytesToRead, FileOptions.WriteThrough);

        //    while (reader.CanRead)
        //    {
        //        var readBytes = reader.Read(buffer, 0, bytesToRead);

        //        if (readBytes == 0) break;

        //        writer.Write(buffer, 0, readBytes);

        //        writer.Flush(true);
        //    }

        //    reader.Close();

        //    reader.Dispose();

        //    writer.Close();

        //    writer.Dispose();
        //}

        // Create File From Stream
        public void CreateFileFromStream(string desPath, Stream stream)
        {
            int bytesToRead = 1024 * 1024 * 2;

            byte[] buffer = new byte[bytesToRead];

            stream.Seek(0, SeekOrigin.Begin);

            FileStream fileStream = File.Create(desPath, bytesToRead, FileOptions.WriteThrough);

            while (fileStream.CanRead)
            {
                var readBytes = stream.Read(buffer, 0, bytesToRead);

                if (readBytes == 0) break;

                fileStream.Write(buffer, 0, readBytes);

                fileStream.Flush(true);
            }

            stream.Close();

            stream.Dispose();

            fileStream.Close();

            fileStream.Dispose();
        }

        // Convert From Stream To Array Byte
        public static byte[] StreamToArrayByte(Stream input)
        {
            byte[] buffer = new byte[1024 * 16];

            using (MemoryStream ms = new MemoryStream())
            {
                int read;

                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }

                return ms.ToArray();
            }
        }

        #endregion
    }
}