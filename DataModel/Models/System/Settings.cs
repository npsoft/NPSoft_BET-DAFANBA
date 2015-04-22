using System;
using System.Linq;
using System.Collections.Generic;
using ServiceStack.OrmLite;
using ServiceStack.DataAnnotations;
using PhotoBookmart.DataLayer.Models.Sites;
using System.Net;
using System.Data;

namespace PhotoBookmart.DataLayer.Models.System
{
    public enum Enum_Settings_DataType
    {
        Raw,
        Int,
        Double,
        Percent,
        String,
        DateTime
    }

    /// <summary>
    /// Define keys to be use to store the settings
    /// </summary>
    public enum Enum_Settings_Key
    {
        NONE,

        #region SMS
        SMS_SERVICE_ENABLE,
        SMS_SERVICE_URL,
        SMS_SERVICE_USERNAME,
        SMS_SERVICE_PASSWORD,
        #endregion

        #region Site Settings
        WEBSITE_CURRENCY,
        WEBSITE_CURRENCY_FORMAT,
        WEBSITE_ADDITIONAL_PAGE_NAME,
        WEBSITE_GST_ENABLE,
        #endregion

        #region FTP Settings
        /// <summary>
        /// Location of the directory to keep all uploaded photobook by FTP (before submit order)
        /// </summary>
        SERVER_DATA_FTP_LOCATION,
        SERVER_DATA_FTP_HOST,
        SERVER_DATA_FTP_USERNAME,
        SERVER_DATA_FTP_PASSWORD,
        SERVER_DATA_FTP_STARTUP_PATH,
        #endregion

        #region File Settings
        /// <summary>
        /// Location for customer to upload files in case corrupt
        /// </summary>
        WEBSITE_CUSTOMER_UPLOAD_PATH_DEFAULT,
        /// <summary>
        /// Place to keep all orders folder which already PAID
        /// </summary>
        WEBSITE_UPLOAD_PATH_DEFAULT,
        /// <summary>
        /// Location to keep all orders folder which is not yet PAY
        /// </summary>
        WEBSITE_ORDERS_FOLDER_NOTYETPAID_PATH,

        /// <summary>
        /// Path to folder which the Digilabs will look into for the Input DGL for decrypt
        /// </summary>
        WEBSITE_DGL_AUTODECRYPT_INPUT,

        /// <summary>
        /// Path to folder which the digilabs will put the output of the decryption
        /// </summary>
        WEBSITE_DGL_AUTODECRYPT_OUTPUT,

        #endregion

        #region Task Settings

        TASK_AUTO_CANCEL_ORDER_IF_MORETHAN_MINUTE

        #endregion
    }

    [Alias("Settings")]
    [Schema("System")]
    public partial class Settings
    {

        [PrimaryKey]
        [AutoIncrement]
        public int Id { get; set; }

        public string Key { get; set; }

        public string Value { get; set; }

        /// <summary>
        /// Get a setting by name
        /// </summary>
        /// <returns>Cast the return to your specific datatype</returns>
        public static object Get(string key, object default_value = null, Enum_Settings_DataType data_type = Enum_Settings_DataType.Raw)
        {
            var db = ModelBase.ServiceAppHost.TryResolve<IDbConnection>();
            if (db.State != ConnectionState.Open)
            {
                db = ModelBase.ServiceAppHost.TryResolve<IDbConnectionFactory>().Open();
            }

            string value = "";

            try
            {
                var k = db.Select<Settings>(x => x.Where(m => m.Key == key).Limit(1)).FirstOrDefault();
                if (k == null)
                {
                    value = default_value.ToString();
                }
                else
                {
                    value = k.Value;
                }

                // // we dispose the connection to save resource
                db.Close();

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

        public static object Get(Enum_Settings_Key key, object default_value = null, Enum_Settings_DataType data_type = Enum_Settings_DataType.Raw)
        {
            return Get(key.ToString(), default_value, data_type);
        }

        /// <summary>
        /// Set data into the db
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="data_type"></param>
        public static void Set(Enum_Settings_Key key, object value, Enum_Settings_DataType data_type = Enum_Settings_DataType.Raw)
        {
            var Db = ModelBase.ServiceAppHost.TryResolve<IDbConnection>();

            if (Db.State != ConnectionState.Open)
            {
                Db = ModelBase.ServiceAppHost.TryResolve<IDbConnectionFactory>().Open();
            }


            // try to get it first
            var key_name = key.ToString();
            var item = Db.Select<Settings>(x => x.Where(m => m.Key == key_name).Limit(1)).FirstOrDefault();
            if (item == null)
            {
                // insert new
                item = new Settings() { Key = key_name, Value = value.ToString() };
                Db.Insert<Settings>(item);
            }
            else
            {
                // update
                item.Value = value.ToString();
                Db.Update<Settings>(item);
            }
            // we dispose the connection to save resource
            Db.Close();
        }
    }
}
