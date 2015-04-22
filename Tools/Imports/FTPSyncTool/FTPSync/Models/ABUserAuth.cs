using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceStack.DataAnnotations;
using ServiceStack.OrmLite;
using ServiceStack.ServiceInterface.Auth;

namespace ABSoft.Photobookmart.FTPSync.Models
{
    public enum RoleEnum { }

    [Alias("Users")]
    public class ABUserAuth : UserAuth
    {
        public bool IsActive { get; set; }

        public ABUserAuth()
        {
            IsActive = false;
        }
    }

    [Alias("UsersOAuth")]
    public class ABUserOAuthProvider : UserOAuthProvider
    {
        public ABUserOAuthProvider() { }
    }
}