using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Data;
using System.Threading;
using System.Net.FtpClient;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Security.Cryptography;
using ABSoft.Photobookmart.FTPSync.Components;
using ABSoft.Photobookmart.FTPSync.Models;

namespace ABSoft.Photobookmart.FTPSync.Helper
{
    /// <summary>
    /// Internal FTP Sync Service
    /// </summary>
    public class FTPHelpers
    {
        #region Variables
        Thread _thread;
        FTPConfig config = new FTPConfig();
        public FtpClient ftpClient = new FtpClient();
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
            get { return _connection; }
        }
        #endregion

        public FTPHelpers(IRBLog log)
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
            config = new FTPConfig()
            {
                FTPHost = ConfigurationManager.AppSettings["FTP_Host"],
                UserName = ConfigurationManager.AppSettings["FTP_User"],
                Password = ConfigurationManager.AppSettings["FTP_Pass"],
                FTPDefaultPath = ConfigurationManager.AppSettings["FTP_Path"],
                SyncsTime = 24 * 60,
                LocalPath = ConfigurationManager.AppSettings["Local_Path"],
                ServerTimeZone = 7,
                LocalTimeZone = 7,
                ConnectionMode = 0,
                DeleteAfterSync = false,
                SyncOrderOnly = true,
                SSLEncryptionMode = 0,
                DataConnectionEncryption = false,
            };
            return config != null;
        }

        public bool LoadConfig(FTPConfig config)
        {
            this.config = config;
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
        public void InitFTPConnection()
        {
            ServicePointManager.ServerCertificateValidationCallback = delegate(object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
            { return true; };

            ftpClient = new FtpClient();

            ftpClient.Host = config.FTPHost;

            ftpClient.Port = config.Port;

            ftpClient.SocketKeepAlive = true;

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
            ftpClient.DataConnectionEncryption = true;

            ftpClient.Credentials = new NetworkCredential(config.UserName, config.Password);

            if (!config.FTPDefaultPath.EndsWith("/"))
            {
                config.FTPDefaultPath += "/";
            }

            ftpClient.ValidateCertificate += new FtpSslValidation(ftpClient_ValidateCertificate);
            syncDurationLength = (24 * 60) / config.SyncsTime;

            log.Log(string.Format("Information\t:: Connecting to FTP Server {0} with username {1}", config.FTPHost, config.UserName));
            ftpClient.Connect();
            ftpClient.SetWorkingDirectory(config.FTPDefaultPath);
            log.Log(string.Format("Information\t:: Connected to FTP Server {0}", config.FTPHost));
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

        public void Start()
        {
            if (!ftpClient.IsConnected && LoadConfig())
            {
                InitFTPConnection();
            }

            if (!config.LocalPath.EndsWith("/"))
            {
                config.LocalPath += "/";
            }
            if (!Directory.Exists(config.LocalPath))
            {
                Directory.CreateDirectory(config.LocalPath);
            }

            SyncThreading();
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
            catch { }
        }

        /// <summary>
        /// Function to run by thread
        /// </summary>
        void SyncThreading()
        {
            #region For: Initialize common variables
            bool is_continue = true;
            string file_client_path = "";
            #endregion

            #region For: Sync file from FTP Server
            log.Log(string.Format("Information\t:: Sync file has been started."));
            string ftp_file = ConfigurationManager.AppSettings["FTP_File"];
            FtpListItem file_ftp = ftpClient
                .GetListing(config.FTPDefaultPath)
                .Where(x =>
                    x.Type == FtpFileSystemObjectType.File &&
                    (string.IsNullOrEmpty(ftp_file) || x.Name == ftp_file) &&
                    new string[1] { ".csv" }.Contains(Path.GetExtension(x.FullName).ToLower()))
                .OrderByDescending(x => x.Modified).FirstOrDefault();
            if (file_ftp != null)
            {
                file_client_path = File_CombinePath(config.LocalPath, file_ftp.Name);
                if (!File.Exists(file_client_path) || !File_Validate(file_ftp, file_client_path))
                {
                    log.Log(string.Format("Information\t:: Start downloading file {0}", file_ftp.FullName));
                    int bytesToRead = 1024 * 300;
                    byte[] buffer = new Byte[bytesToRead];
                    var reader = ftpClient.OpenRead(file_ftp.FullName, FtpDataType.Binary);
                    var writer = File.Create(file_client_path, bytesToRead, FileOptions.WriteThrough);
                    while (reader.CanRead)
                    {
                        var read_bytes = reader.Read(buffer, 0, bytesToRead);
                        if (read_bytes == 0) { break; }
                        writer.Write(buffer, 0, read_bytes);
                        writer.Flush();
                    }
                    writer.Flush(true);
                    reader.Close();
                    reader.Dispose();
                    writer.Close();
                    writer.Dispose();
                    log.Log(string.Format("Information\t:: Downloaded file {0}", file_ftp.FullName));

                    if (!File_Validate(file_ftp, file_client_path, true))
                    {
                        File.Delete(file_client_path);
                        log.Log(string.Format("Error\t:: Deleted file {0}; Compare wrong file size or checksum with server.", file_ftp.FullName));
                        is_continue = false;
                    }
                }
            }
            else
            {
                log.Log(string.Format("Error\t:: File is not found."));
                is_continue = false;
            }
            log.Log(string.Format("Information\t:: Sync file has been finished."));
            #endregion

            if (is_continue)
            {
                log.Log(string.Format("Information\t:: Read file has been started."));
                List<Tuple<string, string>> data = GetDataFromFile(file_client_path, "B", "AE");
                log.Log(string.Format("Information\t:: Retrieved {0} record(s).", data.Count));
                log.Log(string.Format("Information\t:: Read file has been finished."));

                log.Log(string.Format("Information\t:: Update data has been started."));
                foreach (Tuple<string, string> item in data ?? Enumerable.Empty<Tuple<string, string>>()) { }
                log.Log(string.Format("Information\t:: Update data has been finished."));
            }
        }
        #endregion

        #region Copy file from server
        public void CopyFileFromServer(string path_server, string path_local)
        {
            int bytesToRead = 1024 * 300;
            byte[] buffer = new Byte[bytesToRead];
            var reader = ftpClient.OpenRead(path_server, FtpDataType.Binary);
            var writer = File.Create(path_local, bytesToRead, FileOptions.WriteThrough);
            while (reader.CanRead)
            {
                var read_bytes = reader.Read(buffer, 0, bytesToRead);
                if (read_bytes == 0) { break; }
                writer.Write(buffer, 0, read_bytes);
                writer.Flush();
            }
            writer.Flush(true);
            reader.Close();
            reader.Dispose();
            writer.Close();
            writer.Dispose();
        }
        #endregion

        #region Delete file from server
        public void DeleteFileFromServer(string path_server)
        {
            ftpClient.DeleteFile(path_server);
        }
        #endregion

        #region Get index from column name
        public static int GetIdxFromColName(string name)
        {
            int idx = 0;
            int code_frst = (int)'A';
            int code_last = (int)'Z';
            int code_curr = 0;

            string str = name != null ? name.ToUpper() : "";
            for (int i = 0; i < str.Length; i++)
            {
                code_curr = (int)str[i];
                if (code_curr < code_frst || code_curr > code_last)
                {
                    idx = 0;
                    break;
                }
                idx += i * (code_last - code_frst) + code_curr - code_frst + 1;
            }
            return idx - 1;
        }
        #endregion

        #region Get Tracking number and Shipment status from file
        public static List<Tuple<string, string>> GetDataFromFile(string path, string col_tracking, string col_status)
        {
            int idx_tracking = GetIdxFromColName(col_tracking);
            int idx_status = GetIdxFromColName(col_status);
            if (idx_status < 0 || idx_status < 0)
            {
                return new List<Tuple<string, string>>();
            }

            List<Tuple<string, string>> data = new List<Tuple<string, string>>();
            using (CsvFileReader reader = new CsvFileReader(path))
            {
                CsvRow row = new CsvRow();
                while (reader.ReadRow(row))
                {
                    if (row.ValidRow() &&
                        row.Count > idx_status &&
                        row.Count > idx_tracking &&
                        !string.IsNullOrEmpty(row[idx_tracking]))
                    {
                        data.Add(new Tuple<string, string>(row[idx_tracking], row[idx_status]));
                    }
                }
            }
            return data;
        }
        #endregion
    }
}
