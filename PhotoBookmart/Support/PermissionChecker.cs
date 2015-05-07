using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using ServiceStack.Common;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using ServiceStack.OrmLite;

using PhotoBookmart.Areas.Administration.Controllers;
using PhotoBookmart.DataLayer.Models.Products;
using PhotoBookmart.DataLayer.Models.Users_Management;

namespace PhotoBookmart.Helper
{
    /// <summary>
    /// Type Switching, to be use when switch case the type
    /// Sample
    /// var ts = new TypeSwitch()
    ///    .Case((int x) => Console.WriteLine("int"))
    ///    .Case((bool x) => Console.WriteLine("bool"))
    ///    .Case((string x) => Console.WriteLine("string"));
    ///ts.Switch(42);     
    ///ts.Switch(false);  
    ///ts.Switch("hello"); 
    /// </summary>
    public class TypeSwitch
    {
        Dictionary<Type, Func<object, bool>> matches = new Dictionary<Type, Func<object, bool>>();
        public TypeSwitch Case<T>(Func<T, bool> action)
        {
            matches.Add(typeof(T), (x) => { return action((T)x); });
            return this;
        }
        public bool Switch(object x)
        {
            var t = x.GetType();
            if (matches.ContainsKey(t))
            {
                return matches[x.GetType()](x);
            }
            else
            {
                return false;
            }
        }
    }
    /// <summary>
    /// Class to help checking the permission depend on the context and object
    /// </summary>
    public class PermissionChecker
    {
        WebAdminController service;
        TypeSwitch canUpdate;
        TypeSwitch canAdd;
        TypeSwitch canGet;
        TypeSwitch canList;
        TypeSwitch canStatistic;
        public PermissionChecker(WebAdminController service)
        {
            this.service = service;
            // init the type switcher 
            //canUpdate = new TypeSwitch()
            //        .Case((ProspectCustomer x) => { return this.CanUpdate(x); })
            //        .Case((KIT x) => { return this.CanUpdate(x); });

            // for add
            canAdd = new TypeSwitch()
                   .Case((DoiTuong x) => { return this.CanAdd(x); });

            // for Get
            canUpdate = new TypeSwitch()
                   .Case((DoiTuong x) => { return this.CanUpdate(x); });

            //// for List
            //canList = new TypeSwitch()
            //       .Case((KIT x) => { return this.CanList(x); });

            //// for Statistic
            //canStatistic = new TypeSwitch()
            //       .Case((KIT x) => { return this.CanStatistic(x); });
        }

        #region Functions
        /// <summary>
        /// Return true if the current user in this context has permission to do the udpate
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool CanUpdate<T>(T item)
        {
            return canUpdate.Switch(item);
        }

        public bool CanAdd<T>(T item)
        {
            return canAdd.Switch(item);
        }

        public bool CanDelete<T>(T item)
        {
            var ret = false;


            return ret;
        }

        public bool CanGet<T>(T item)
        {
            return canGet.Switch(item);
        }

        //bool Check_A_isLeader_of_B(ABUserAuth A, ABUserAuth B)
        //{
        //    // always consume that A will have greater permission than B
        //    if (A.Id == B.Id)
        //    {
        //        return true;
        //    }
        //    // now we get all teams that A has the A_Role
        //    var Db = service.Db;
        //    var ret = false;

        //    var A_teams = Db.Where<Team_vs_User>(x => x.UserId == A.Id && x.Type != (int)Enum_Team_vs_User_Type.MarketArea);

        //    // if A is Area Manager, we have to get all sub teams
        //    if (A_teams.Count == 0)
        //    {
        //        return false; // there is no teams, actually this is false
        //    }
        //    var A_teams_Id = A_teams.Where(x => x.TypeEnum == Enum_Team_vs_User_Type.AreaManager).Select(x => x.TeamId); // for sure the array will have elements
        //    if (A_teams_Id.Count() > 0)
        //    {
        //        var A_sub_teams = Db.Select<TeamGroup>(x => x.Where(y => y.isTeam == false && Sql.In(y.ParentId, A_teams_Id) && y.ParentId > 0));
        //        A_sub_teams.ForEach(x =>
        //        {
        //            if (A_teams.Count(y => y.TeamId == x.Id) == 0)
        //            {
        //                A_teams.Add(new Team_vs_User()
        //                {
        //                    Id = 0,
        //                    TeamId = x.Id,
        //                    UserId = A.Id,
        //                    TypeEnum = Enum_Team_vs_User_Type.AreaManager
        //                });
        //            }
        //        });
        //    }


        //    var B_teams = Db.Where<Team_vs_User>(x => x.UserId == B.Id && x.Type != (int)Enum_Team_vs_User_Type.MarketArea);

        //    if (A_teams.Count(x => x.TypeEnum == Enum_Team_vs_User_Type.SaleManager) > 0)
        //    {
        //        // for A has permission Sale Manager, let's check B 
        //        if (B_teams.Count(x => x.TypeEnum == Enum_Team_vs_User_Type.SaleManager) > 0)
        //        {
        //            // both A and B are Sale Manager, we can not allow A to see B
        //            return false;
        //        }
        //        else
        //        {
        //            if ((B_teams.Count(x => x.TypeEnum == Enum_Team_vs_User_Type.AreaManager || x.TypeEnum == Enum_Team_vs_User_Type.TeamLeader || x.TypeEnum == Enum_Team_vs_User_Type.Consultant) > 0)
        //                || B.HasRole(RoleEnum.Sales_Consultant, RoleEnum.Sales_AreaManager, RoleEnum.Sales_TeamLeader))
        //            {
        //                // now we are surely know A has permission on B coz A is Sale Manager
        //                return true;
        //            }
        //        }
        //    }

        //    // now we try to match from A_teams to B_Teams, whether any element in b_teams can match A teams, we return true;
        //    foreach (var bt in B_teams)
        //    {
        //        var match = A_teams.Where(x => x.TeamId == bt.TeamId).FirstOrDefault();
        //        if (match != null)
        //        {
        //            // match (A) vs bt (B)
        //            // bt in A_teams? let's check it
        //            if (match.TypeEnum == Enum_Team_vs_User_Type.AreaManager
        //                && (bt.TypeEnum == Enum_Team_vs_User_Type.TeamLeader || bt.TypeEnum == Enum_Team_vs_User_Type.Consultant))
        //            {
        //                ret = true;
        //                break;
        //            }

        //            if (match.TypeEnum == Enum_Team_vs_User_Type.TeamLeader
        //                && bt.TypeEnum == Enum_Team_vs_User_Type.Consultant)
        //            {
        //                ret = true;
        //                break;
        //            }
        //        }
        //    }
        //    return ret;
        //}
        #endregion

        //#region Prospect Customer
        ///**
        //     * Direct Team leader
        //     * Direct Area Manager
        //     * Admin 
        //     * Sale Manager
        //     * Owner
        //     */
        //private bool CanUpdate(ProspectCustomer item)
        //{
        //    if (item == null)
        //    {
        //        return false;
        //    }

        //    var u = service.CurrentUser;
        //    if (item.CreatedBy == u.Id)
        //    {
        //        return true;
        //    }

        //    // check whether they are admin
        //    if (u.IsAdmin())
        //    {
        //        return true;
        //    }

        //    if (u.HasRole(RoleEnum.Sales_AreaManager, RoleEnum.Sales_TeamLeader, RoleEnum.Sales_Manager))
        //    {
        //        var user_item = service.Db.Select<ABUserAuth>(x => x.Where(y => y.Id == item.CreatedBy).Limit(1)).FirstOrDefault();
        //        if (user_item == null)
        //        {
        //            return false;
        //        }
        //        else
        //        {
        //            return Check_A_isLeader_of_B(u, user_item);
        //        }
        //    }

        //    return false;
        //}

        //private bool CanAdd(ProspectCustomer item)
        //{
        //    // for Prospect, anyone can add
        //    return true;
        //}

        //private bool CanGet(ProspectCustomer item)
        //{
        //    if (item == null)
        //    {
        //        return false;
        //    }
        //    var u = service.CurrentUser;
        //    if (item.CreatedBy == u.Id)
        //    {
        //        return true;
        //    }

        //    // check whether they are admin
        //    if (u.IsAdmin())
        //    {
        //        return true;
        //    }

        //    if (u.HasRole(RoleEnum.Sales_AreaManager, RoleEnum.Sales_TeamLeader, RoleEnum.Sales_Manager))
        //    {
        //        var user_item = service.Db.Select<ABUserAuth>(x => x.Where(y => y.Id == item.CreatedBy).Limit(1)).FirstOrDefault();
        //        if (user_item == null)
        //        {
        //            return false;
        //        }
        //        else
        //        {
        //            return Check_A_isLeader_of_B(u, user_item);
        //        }
        //    }


        //    return false;
        //}
        //#endregion

        #region TODO: Đối tượng

        private bool CanView(DoiTuong item)
        {
            return true;
            /* TODO: Comments
            if (item == null)
            {
                return false;
            }

            var u = service.CurrentUser;
            return (u.HasRole(RoleEnum.Administrator) ||
                u.HasRole(RoleEnum.Super_Manager) ||
                u.HasRole(RoleEnum.Managing_Director) ||
                (u.HasRole(RoleEnum.Sales_Consultant) && u.Id == item.Assigned_User_Id));*/
        }

        private bool CanAdd(DoiTuong item)
        {
            if (item == null) { return false; }
            ABUserAuth curr_user = service.CurrentUser;
            return curr_user.HasRole(RoleEnum.Admin) || curr_user.MaHC.StartsWith(item.MaHC);
        }

        private bool CanUpdate(DoiTuong item)
        {
            if (item == null) { return false; }
            ABUserAuth curr_user = service.CurrentUser;
            return curr_user.HasRole(RoleEnum.Admin) || curr_user.MaHC.StartsWith(item.MaHC);
        }

        //private bool CanList(KIT item)
        //{
        //    var u = service.CurrentUser;
        //    if (u.HasRole(RoleEnum.Sales_Consultant))
        //    {
        //        return true;
        //    }

        //    return false;
        //}

        //private bool CanStatistic(KIT item)
        //{
        //    var u = service.CurrentUser;
        //    if (u.HasRole(RoleEnum.Sales_Consultant))
        //    {
        //        return true;
        //    }

        //    return false;
        //}

        #endregion
    }
}
