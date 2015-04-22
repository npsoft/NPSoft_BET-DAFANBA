using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ABSoft.Photobookmart.CleanOldPhotobook.ServiceInterface;
using System.Windows.Forms;
using System.Data;
using ABSoft.Photobookmart.CleanOldPhotobook.Components;

namespace ABSoft.Photobookmart.CleanOldPhotobook
{
    public static class AppHost 
    {
        /// <summary>
        ///  Share FTP Service among other threads
        /// </summary>
        public static PhotobookmartService Service { get; set; }

        public static IRBLog Log { get; set; }

        public static string ServiceName
        {
            get
            {
                return "Photobookmart";
            }
        }

        public static string ServiceDescription
        {
            get
            {
                return "Clean old photobooks, auto send payment email, auto cancel order, auto decrypt DGL file";
            }
        }
    }
}