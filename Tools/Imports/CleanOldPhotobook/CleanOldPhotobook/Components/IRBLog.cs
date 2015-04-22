using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ABSoft.Photobookmart.CleanOldPhotobook.Components
{
    public interface IRBLog : IDisposable
    {
        bool LogToConsole { get; }
        void Console_Writeline(string st, bool datetime_included);
        void Console_Write(string st, bool datetime_included);
        void Log(string st, bool datetime_included = true);
        void Log(Exception ex);
    }
}
