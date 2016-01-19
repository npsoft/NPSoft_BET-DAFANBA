using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceProcess;

namespace ABSoft.Photobookmart.FTPSync.ServiceInterface
{
    public partial class FTPSyncWindowsService : ServiceBase
    {
        private FTPSyncService service;

        public FTPSyncWindowsService(FTPSyncService service)
        {
            this.service = service;
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                service.Stop();
            }
            catch
            {
            }
        }

        protected override void OnStart(string[] args)
        {
            //this.service = new FTPSyncService();
            service.Start();
        }

        protected override void OnStop()
        {
            service.Stop();
        }

        protected override void OnShutdown()
        {
            service.Stop();
        }
    }
}
