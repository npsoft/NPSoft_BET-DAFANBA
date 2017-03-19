using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;

namespace SpiralEdge.Helper
{
    /// <summary>
    /// Support log
    /// </summary>
    public class RBLog : IRBLog
    {
        private StreamWriter w;
        public string Path_Log { get; set; }
        public bool LogToConsole { get; private set; }
        private List<string> Queue { get; set; }
        private int Queue_Ex_Times { get; set; }
        private int Queue_Ex_TimeWait { get; set; }
        private static readonly object padlock = new object();
        
        public RBLog()
        {
            LogToConsole = true;
            Queue = new List<string>();
            Queue_Ex_Times = 5;
            Queue_Ex_TimeWait = 1 * 60 * 1000;
        }

        public RBLog(string path)
        {
            Path_Log = path;
            LogToConsole = true;
            Queue = new List<string>();
            Queue_Ex_Times = 5;
            Queue_Ex_TimeWait = 1 * 60 * 1000;
        }

        public void Close()
        {
            try
            {
                w.Close();
            }
            catch { }
        }

        public void Dispose()
        {
            try
            {
                Log(string.Format("Information\t:: ------------------ APPLICATION CLOSING -----------------------"));
                w.Dispose();
            }
            catch { }
        }

        public virtual void Console_Write(string content, bool inc_time = true)
        {
            Console.Write(inc_time ? string.Format("{0:MM/dd/yyyy HH:mm:ss}\t:: {1}", DateTime.Now, content) : string.Format("{0}", content));
            System.Diagnostics.Debug.Print(inc_time ? string.Format("{0:MM/dd/yyyy HH:mm:ss}\t:: {1}", DateTime.Now, content) : string.Format("{0}", content));
        }

        public virtual void Console_WriteLine(string content, bool inc_time = true)
        {
            Console.WriteLine(inc_time ? string.Format("{0:MM/dd/yyyy HH:mm:ss}\t:: {1}", DateTime.Now, content) : string.Format("{0}", content));
            System.Diagnostics.Debug.Print(inc_time ? string.Format("{0:MM/dd/yyyy HH:mm:ss}\t:: {1}", DateTime.Now, content) : string.Format("{0}", content));
        }

        public virtual void Log(string content, string path = null)
        {
            lock (padlock)
            {
                bool is_ex = false;
                Console_WriteLine(content, true);
                path = string.IsNullOrEmpty(path) ? Path_Log : path;
                Queue.Add(string.Format("{0:MM/dd/yyyy HH:mm:ss}\t:: {1}", DateTime.Now, content));
                if (!string.IsNullOrEmpty(path))
                {
                    try
                    {
                        UTF8Encoding utf8WithoutBom = new UTF8Encoding(false);
                        w = new StreamWriter(path, true, utf8WithoutBom);
                        do
                        {
                            w.WriteLine(Queue[0]);
                            Queue.RemoveAt(0);
                        } while (0 != Queue.Count);
                        w.Close();
                    }
                    catch (Exception ex)
                    {
                        is_ex = true;
                        for (int t = 0; t < Queue_Ex_Times; t++)
                        {
                            try
                            {
                                System.Threading.Thread.Sleep(Queue_Ex_TimeWait);
                                UTF8Encoding utf8WithoutBom = new UTF8Encoding(false);
                                w = new StreamWriter(path, true, utf8WithoutBom);
                                do
                                {
                                    w.WriteLine(Queue[0]);
                                    Queue.RemoveAt(0);
                                } while (0 != Queue.Count);
                                w.Close();
                                is_ex = false;
                                break;
                            }
                            catch (Exception ex2) { Console_WriteLine(string.Format("LOG EXCEPTION\t:: {0}{1}", ex2.Message, ex2.StackTrace), false); }
                        }
                        if (is_ex) { throw new Exception(string.Format("{0}{1}", ex.Message, ex.StackTrace), ex); }
                    }
                }
            }
        }

        public virtual void Log(Exception ex, string path = null)
        {
            path = string.IsNullOrEmpty(path) ? Path_Log : path;
            Log(string.Format("Exception\t:: {0}", ex.Message), path);
            Log(string.Format("StackTrace\t:: {0}", ex.StackTrace), path);
        }
    }
}
