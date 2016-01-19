using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using ServiceStack.OrmLite;
using System.Threading;
using ABSoft.Photobookmart.FTPSync.Models;
using System.Net.FtpClient;
using System.Net;
using ABSoft.Photobookmart.FTPSync.Components;
using System.IO;
using System.Text.RegularExpressions;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Security.Cryptography;

namespace ABSoft.Photobookmart.FTPSync.ServiceInterface
{
    /// <summary>
    /// Internal FTP Sync Service
    /// </summary>
    public class FTPSyncService
    {
        #region Variables
        Thread _thread;
        FTPConfig config = new FTPConfig();
        FtpClient ftpClient = new FtpClient();
        IRBLog log;

        /// <summary>
        /// how many minutes each time we wait for next sync
        /// </summary>
        int syncDurationLength = 1;
        #endregion

        #region Public Property
        public bool IsConnected
        {
            get
            {
                return ftpClient.IsConnected;
            }
        }

        public bool IsSupportHASH
        {
            get
            {
                return ftpClient.IsConnected && ftpClient.HashAlgorithms != FtpHashAlgorithm.NONE;
            }
        }

        /// <summary>
        /// Return true if service is running the sync
        /// </summary>
        public bool IsRunningSync { get; private set; }

        IDbConnection _connection = null;
        public virtual IDbConnection Db
        {
            get
            {
                if (_connection == null)
                {
                    _connection = AppHost.Resolve<IDbConnection>();

                }

                if (_connection != null && _connection.State != ConnectionState.Open)
                {
                    // force open new connection
                    _connection = AppHost.Resolve<IDbConnectionFactory>().Open();
                }

                return _connection;
            }
        }
        #endregion

        public FTPSyncService(IRBLog log)
        {
            this.log = log;
        }

        #region Database & Data Functions


        /// <summary>
        /// Load FTP Config
        /// </summary>
        /// <returns></returns>
        bool LoadConfig()
        {
            config = Db.Select<FTPConfig>(x => x.Limit(1)).FirstOrDefault();
            return config != null;
        }


        #endregion

        #region Files & Folder
        /// <summary>
        /// Return true if folder is in format XXXXXX with X is digit
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        bool OrderFolderNameValidate(string name)
        {
            if (config.SyncOrderOnly)
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
            else
            {
                return true;
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

        #region FTP Functions
        private void InitFTPConnection()
        {
            try
            {
                ServicePointManager.ServerCertificateValidationCallback = delegate(object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
                { return true; };

                ftpClient = new FtpClient();

                ftpClient.Host = config.FTPHost;

                ftpClient.Port = config.Port;

                ftpClient.SocketKeepAlive = true;

                // connection mode
                switch (config.ConnectionMode)
                {
                    case 0:
                        // auto
                        break;
                    case 1:
                        // Active
                        ftpClient.DataConnectionType = FtpDataConnectionType.AutoActive;
                        break;
                    case 2:
                        // passive
                        ftpClient.DataConnectionType = FtpDataConnectionType.EPSV;
                        break;
                    default:
                        //ftpClient.DataConnectionType = FtpDataConnectionType.AutoPassive;
                        break;
                }

                ftpClient.EncryptionMode = (FtpEncryptionMode)config.SSLEncryptionMode;

                ftpClient.Credentials = new NetworkCredential(config.UserName, config.Password);

                if (!config.FTPDefaultPath.EndsWith("/"))
                {
                    config.FTPDefaultPath += "/";
                }

                ftpClient.ValidateCertificate += new FtpSslValidation(ftpClient_ValidateCertificate);
                syncDurationLength = (24 * 60) / config.SyncsTime;

                log.Log("Connecting to FTP Server " + config.FTPHost + " with username " + config.UserName);
                ftpClient.Connect();
                ftpClient.SetWorkingDirectory(config.FTPDefaultPath);
                log.Log("Connected to FTP Server " + config.FTPHost);
                log.Log("Server HASH support: " + ftpClient.HashAlgorithms.ToString());
            }
            catch (Exception ex)
            {
                log.Log("Can not connect to FTP host" + config.FTPHost);
                log.Log(ex);
            }
        }

        void ftpClient_ValidateCertificate(FtpClient control, FtpSslValidationEventArgs e)
        {
            e.Accept = true;
        }

        /// <summary>
        /// 
        /// </summary>
        void FTP_Disconnect()
        {
            if (ftpClient.IsConnected)
            {
                ftpClient.Disconnect();
            }
        }

        public bool Verify(string hash, string file, FtpHashAlgorithm m_algorithm)
        {
            using (FileStream istream = new FileStream(file, FileMode.Open, FileAccess.Read))
            {
                return Verify(istream, hash, m_algorithm);
            }
        }

        /// <summary>
        /// Hash verify
        /// </summary>
        /// <param name="istream"></param>
        /// <returns></returns>
        bool Verify(Stream istream, string input_hash, FtpHashAlgorithm m_algorithm)
        {

            HashAlgorithm hashAlg = null;

            switch (m_algorithm)
            {
                case FtpHashAlgorithm.SHA1:
                    hashAlg = new SHA1CryptoServiceProvider();
                    break;
#if !NET2
                case FtpHashAlgorithm.SHA256:
                    hashAlg = new SHA256CryptoServiceProvider();
                    break;
                case FtpHashAlgorithm.SHA512:
                    hashAlg = new SHA512CryptoServiceProvider();
                    break;
#endif
                case FtpHashAlgorithm.MD5:
                    hashAlg = new MD5CryptoServiceProvider();
                    break;
                case FtpHashAlgorithm.CRC:
                    throw new NotImplementedException("There is no built in support for computing CRC hashes.");
                default:
                    throw new NotImplementedException("Unknown hash algorithm: " + m_algorithm.ToString());
            }

            try
            {
                byte[] data = null;
                string hash = "";

                data = hashAlg.ComputeHash(istream);
                if (data != null)
                {
                    foreach (byte b in data)
                    {
                        hash += b.ToString("x2");
                    }

                    return (hash.ToUpper() == input_hash.ToUpper());
                }
            }
            finally
            {
#if !NET2 // .NET 2.0 doesn't provide access to Dispose() for HashAlgorithm
                if (hashAlg != null)
                    hashAlg.Dispose();
#endif
            }

            return false;
        }

        /// <summary>
        /// Validate file to compare file on local and on ftp server
        /// </summary>
        /// <param name="ftp_path"></param>
        /// <param name="local_file_path"></param>
        /// <returns></returns>
        bool File_Validate(FtpListItem dir1_file, string local_file_path, bool validate_with_Hash = false)
        {
            FileInfo current_fileinfo = new FileInfo(local_file_path);

            if (current_fileinfo.Length != dir1_file.Size)
            {
                return false;
            }
            else
            {
                // only chekc if server support for HASH
                if (validate_with_Hash && ftpClient.HashAlgorithms != FtpHashAlgorithm.NONE)
                {
                    // same filename? not sure, need to check the hash CRC
                    var hash = ftpClient.GetHash(dir1_file.FullName);
                    if (hash.IsValid)
                    {
                        if (!hash.Verify(local_file_path))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        log.Log("Verify file " + dir1_file.FullName + " failed. Wrong Hash returned from server. File ignore.");
                    }
                    Sleep();
                }
                else
                {
                    return true;
                }
            }
            return true;
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
            // if is running then no need to do 
            if (ftpClient.IsConnected)
            {

            }

            if (LoadConfig())
            {
                InitFTPConnection();
            }

            // check local directory
            if (!config.LocalPath.EndsWith("/"))
            {
                config.LocalPath += "/";
            }
            if (!Directory.Exists(config.LocalPath))
            {
                Directory.CreateDirectory(config.LocalPath);
            }

            // start the thread to wait for each minutes
            _thread = new Thread(new ThreadStart(SyncThreading));
            _thread.Priority = ThreadPriority.Highest;
            _thread.Start();
        }

        /// <summary>
        /// Stop the service 
        /// </summary>
        public void Stop()
        {
            try
            {
                _thread.Abort();
                _connection.Close();
                FTP_Disconnect();
            }
            catch
            {
            }
        }

        /// <summary>
        /// Function to run by thread
        /// </summary>
        int _syncthreading_last_cmins = 0;
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
                    var cmins = DateTime.Now.Hour * 60 + DateTime.Now.Minute;
                    if (cmins != _syncthreading_last_cmins && cmins % syncDurationLength == 0 && !IsRunningSync)
                    {
                        lock (_thread)
                        {
                            IsRunningSync = true;
                        }
                        log.Log("Sync has been started");
                        // update the last cmins
                        _syncthreading_last_cmins = cmins;

                        // check server to sync
                        var remote_folder_list = ftpClient.GetListing(config.FTPDefaultPath).Where(x =>
                            x.Type == FtpFileSystemObjectType.Directory && !Sql.In(x.Name, blacklist_foldername));

                       
                        foreach (var dir1 in remote_folder_list)
                        {
                            try
                            {
                                // finish this directory? check to delete if expiration or delete on success
                                if (DateTime.Now.Subtract(dir1.Modified).TotalDays > 30 * config.DeleteExpiredMonths)
                                {
                                    // delete
                                    try
                                    {
                                        ftpClient.DeleteDirectory(dir1.FullName, true);
                                    }
                                    catch (Exception ex)
                                    {
                                        log.Log("Delete directory " + dir1.FullName + " not success.");
                                        log.Log(ex);
                                    }
                                    continue;
                                }

                                // check directory foldername
                                if (!OrderFolderNameValidate(dir1.Name))
                                {
                                    continue;
                                }

                                // check this directory existing
                                DirectoryInfo dir1_info = null;
                                try
                                {
                                    dir1_info = File_CreateDirectory(config.LocalPath, dir1.Name);
                                }
                                catch (Exception ex)
                                {
                                    log.Log("Can create directory " + dir1.Name + " on " + config.LocalPath);
                                    log.Log(ex);
                                    continue;
                                }

                                //try
                                //{
                                //    dir1_info.LastWriteTime = dir1.Modified;
                                //    dir1_info.CreationTime = dir1.Created;
                                //}
                                //catch (Exception ex)
                                //{
                                //    log.Log("Can not update last modify and create time for  " + dir1.Name);
                                //    log.Log(ex);
                                //}

                                Sleep();

                                // check files in this directory
                                var dir1_files = ftpClient.GetListing(dir1.FullName).Where(x =>
                                    x.Type == FtpFileSystemObjectType.File && !Sql.In(x.Name, blacklist_foldername));
                                var dir1_file_success_count = 0;
                                foreach (var dir1_file in dir1_files)
                                {
                                    bool canDownload = false;
                                    var current_dir_path = dir1_info.FullName;

                                    var current_filepath = File_CombinePath(current_dir_path, dir1_file.Name);

                                    try
                                    {
                                        // check file existing
                                        if (!File.Exists(current_filepath))
                                        {
                                            canDownload = true;
                                        }
                                        else
                                        {
                                            // if existing? then check the filesize first and then the CRC
                                            canDownload = !File_Validate(dir1_file, current_filepath);
                                        }

                                        // if need download the file?
                                        if (canDownload)
                                        {
                                            try
                                            {
                                                log.Log("Start downloading file " + dir1_file.FullName + " to " + current_filepath);
                                                int bytesToRead = 1024 * 300;
                                                byte[] buffer = new Byte[bytesToRead];

                                                // download the file again
                                                var reader = ftpClient.OpenRead(dir1_file.FullName, FtpDataType.Binary);
                                                var writer = File.Create(current_filepath, bytesToRead, FileOptions.WriteThrough);
                                                while (reader.CanRead)
                                                {
                                                    var read_bytes = reader.Read(buffer, 0, bytesToRead);
                                                    if (read_bytes == 0)
                                                    {
                                                        break;
                                                    }

                                                    writer.Write(buffer, 0, read_bytes);
                                                    writer.Flush();
                                                }
                                                writer.Flush(true);
                                                reader.Close();
                                                reader.Dispose();
                                                writer.Close();
                                                writer.Dispose();
                                                log.Log("File " + dir1_file.FullName + " downloaded ");

                                                // validate file again
                                                if (File_Validate(dir1_file, current_filepath, true))
                                                {
                                                    // same file content, then decide what next
                                                    if (config.DeleteAfterSync)
                                                    {
                                                        dir1_file_success_count++;
                                                    }
                                                    log.Log("File " + dir1_file.FullName + " validated");
                                                }
                                                else
                                                {
                                                    // delete on local for this file has wrong content
                                                    log.Log("DELETE " + current_filepath + " ; compare wrong file size or checksum with server");
                                                    File.Delete(current_filepath);
                                                }

                                            }
                                            catch (Exception ex)
                                            {
                                                log.Log("Error while downloading file " + dir1_file.FullName);
                                                log.Log(ex);
                                            }
                                        }
                                        else
                                        {
                                            dir1_file_success_count++;
                                        }

                                        //try
                                        //{
                                        //    var f1_info = new FileInfo(current_filepath);
                                        //    f1_info.LastWriteTime = dir1_file.Modified;
                                        //    f1_info.CreationTime = dir1_file.Created;
                                        //}
                                        //catch (Exception ex)
                                        //{
                                        //    log.Log("Can not update last modify and create time for  " + current_filepath);
                                        //    log.Log(ex);
                                        //}
                                    }
                                    catch (Exception ex)
                                    {
                                        log.Log("Error while processing with file " + dir1_file.FullName);
                                        log.Log(ex);
                                    }

                                    //
                                    Sleep();
                                }

                                Sleep();

                                // finish this directory? check to delete if expiration or delete on success
                                if (config.DeleteAfterSync && dir1_files.Count() == dir1_file_success_count)
                                {
                                    // delete
                                    try
                                    {
                                        ftpClient.DeleteDirectory(dir1.FullName, true);
                                    }
                                    catch (Exception ex)
                                    {
                                        log.Log("Delete directory " + dir1.FullName + " not success.");
                                        log.Log(ex);
                                    }
                                }
                            }
                            catch (Exception ex)  // catch when working with directory
                            {
                                // 
                                log.Log(ex);
                                continue;
                            }
                        } // end dir foreach

                        // done? then set no Sync
                        IsRunningSync = false;
                        log.Log("Sync has been finished");
                    }
                }
                catch (Exception ex)
                {
                    log.Log(ex);
                    IsRunningSync = false;
                    try
                    {
                        ftpClient.Dispose();
                    }
                    catch
                    {
                    }
                    InitFTPConnection();
                }
                Thread.Sleep(1000 * 60); // sleep in one minute
            }
        }
        #endregion

    }
}
