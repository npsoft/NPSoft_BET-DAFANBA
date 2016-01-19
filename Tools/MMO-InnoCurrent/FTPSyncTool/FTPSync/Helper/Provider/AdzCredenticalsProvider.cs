using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using ServiceStack.Common.Web;
using ServiceStack.OrmLite;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceInterface.Auth;

using ABSoft.Photobookmart.FTPSync.Models;

namespace ABSoft.Photobookmart.FTPSync.Helper.Provider
{
    public partial class AdzCredenticalsProvider : CredentialsAuthProvider
    {
        public IDbConnectionFactory DbFactory { get; set; }

        private IDbConnection db;

        public virtual IDbConnection Db
        {
            get { return db ?? (db = DbFactory.Open()); }
        }

        public override bool TryAuthenticate(IServiceBase authService, string userName, string password)
        {
            if (DbFactory == null)
            {
                DbFactory = authService.TryResolve<IDbConnectionFactory>();
            }

            if (Db.Count<ABUserAuth>(x => (x.UserName == userName && x.IsActive)) != 0)
            {
                return base.TryAuthenticate(authService, userName, password);
            }

            return false;
        }

        public override object Authenticate(IServiceBase authService, IAuthSession session, Auth request)
        {
            if (DbFactory == null)
            {
                DbFactory = authService.TryResolve<IDbConnectionFactory>();
            }

            if (Db.Count<ABUserAuth>(x => (x.UserName == request.UserName && x.IsActive)) != 0)
            {
                return base.Authenticate(authService, session, request);
            }

            throw HttpError.Unauthorized("Your account is not actived. Please active your account or contact our Support Team. Thanks");
        }

        public override void OnAuthenticated(IServiceBase authService, IAuthSession session, IOAuthTokens tokens, Dictionary<string, string> authInfo)
        {
            base.OnAuthenticated(authService, session, tokens, authInfo);
        }
    }
}