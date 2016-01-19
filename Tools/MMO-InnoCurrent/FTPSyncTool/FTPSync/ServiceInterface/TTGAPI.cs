using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceStack.ServiceInterface;

namespace ABSoft.Photobookmart.FTPSync.ServiceInterface
{
    public class TTGAPI : Service, IDisposable
    {
        public override void Dispose()
        {
            try
            {
                if (Db != null)
                {
                    Db.Close();
                }
            }
            catch
            {

            }
            base.Dispose();
            GC.SuppressFinalize(this);
        }


    }
}