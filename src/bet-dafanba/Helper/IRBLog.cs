using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpiralEdge.Helper
{
    public interface IRBLog : IDisposable
    {
        bool LogToConsole { get; }
        void Console_Write(string content, bool inc_time);
        void Console_WriteLine(string content, bool inc_time);
        void Log(string content, string path = null);
        void Log(Exception ex, string path = null);
    }
}
