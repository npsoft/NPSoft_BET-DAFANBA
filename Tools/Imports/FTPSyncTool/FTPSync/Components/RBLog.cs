using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ABSoft.Photobookmart.FTPSync.Components
{
    /// <summary>
    /// Support log
    /// </summary>
    public class RBLog : IRBLog
    {
        StreamWriter w;
        /// <summary>
        /// Return true if log all to console
        /// </summary>
        public bool LogToConsole { get; private set; }

        public RBLog()
        {
            LogToConsole = true;
        }

        public void Dispose()
        {
            try
            {
                Log("----------------------APP CLOSING-------------------");
                w.Close();
            }
            catch
            {
            }
        }

        public void Close()
        {
            try
            {
                w.Close();
            }
            catch
            {
            }
        }

        public virtual void Console_Writeline(string st, bool datetime_included)
        {
            if (datetime_included)
            {
                st = DateTime.Now.ToString() + " " + st;
            }
            Console.WriteLine(st);
        }

        public virtual void Console_Write(string st, bool datetime_included)
        {
            if (datetime_included)
            {
                st = DateTime.Now.ToString() + " " + st;
            }
            Console.Write(st);
        }

        public virtual void Log(string st, bool datetime_included = true)
        {
            if (LogToConsole)
            {
                Console_Writeline(st, datetime_included);
            }
            try
            {
                if (datetime_included)
                {
                    st = DateTime.Now.ToString() + " " + st;
                }
                w = File.AppendText(System.Windows.Forms.Application.StartupPath + "\\log.txt");
                w.WriteLine(st);
                w.Close();
            }
            catch
            {
            }
        }

        public virtual void Log(Exception ex)
        {
            Log("EXCEPTION:" + ex.Message, true);
            Log("StackTrace: \r\n" + ex.StackTrace, true);
        }
    }
}
