 using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.SessionState;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.ViewModel;
using GeoAppBuilder.BusinessLayer;
using GeoAppBuilder.Classes;
using GeoAppBuilder.MemberPages.WebUserControl;
using GeoAppBuilder.Models;
using Microsoft.AspNet.Identity;

namespace GeoAppBuilder.ViewModels
{
    public class SiteViewModel : DotvvmViewModelBase
    {

        public ApplicationUserManager userManager = new ApplicationUserManager();

        GeneralBL bl = new GeneralBL();
       
        protected int timeout;
        GeoAppSession geoAppSession;
        
        public ApplicationUser thisUser = null;

        public string UserName { get; set; } = "";

        public bool isEnterprise { get; set; } = GeoApp.Config.ConfigSettings.Instance.EnterpriseInstall;

        public string baseUrl { get; set; } = "";
        public bool isAdmin { get; set; } = false;

        public bool isDigitisor { get; set; } = false;

        public bool isSuperAdmin { get; set; } = false;

        public string FreeTrialDaysUrl { get; set; }

        public string FreeTrialDaysMsg { get; set; }

        public string LogInLogOut { get; set; } = "Log In";

        public string LogInLogOutImage { get; set; } = "/images/key.png";

        private HttpSessionStateWrapper session;


        public override Task Init()
        {
            bl.Init(Context);
            if (session == null)
                session = DotvvmGeneric.GetSessionWrapper(Context.GetOwinContext());
            
            userManager = new ApplicationUserManager();
            
            timeout = (session.Timeout * 60000) - 60000;
            if (session["SessionId"] == null)
            {
                if (session != null)
                {
                    session["SessionId"] = System.Web.HttpContext.Current.Session.SessionID;
                    if (session["BaseURL"] != null)
                        baseUrl = session["BaseURL"].ToString();
                }

            }


            if (session["AzimapUserName"] != null)
            {
                if (!String.IsNullOrEmpty(session["AzimapUserName"].ToString()))
                {
                    thisUser = userManager.FindByName(session["AzimapUserName"].ToString());
                    
                    SetLoggedIn(thisUser);
                }
                




                Logger.Log("user: " + session["AzimapUserName"]);
                if (bool.Parse(WebConfigurationManager.AppSettings["UseAdAuthentication"]) == false && String.IsNullOrEmpty(session["AzimapUserName"].ToString()))
                {
                    ApplicationUser mu = userManager.FindById(HttpContext.Current.User.Identity.GetUserId());
                    if (mu != null)
                    {
                        Logger.Log("muuser: " + mu.UserName.ToString());
                        session["AzimapUserName"] = mu.UserName.ToString();
                        thisUser = userManager.FindById(mu.Id);
                        //UserName = thisUser.UserName.Split('_')[0];
                        SetLoggedIn(thisUser);
                    }
                }
                if (!String.IsNullOrEmpty(session["AzimapUserName"].ToString()))
                {
                    var user = userManager.FindByName(session["AzimapUserName"].ToString());
                    if (userManager.IsInRole(user.Id, "SuperAdmin"))
                    {
                        //superadmin.Visible = true;
                    }
                    
                    if (HttpContext.Current.Request.Url.ToString().Contains("/AdminPages/") && !userManager.IsInRole(user.Id, "Admin"))
                    {
                        //Response.Redirect("~/Home.aspx");
                    }
                    UserName = user.UserName.Split('_')[0];
                }
                
            }
            if (!Context.IsPostBack)
            {
                geoAppSession = GeoAppSession.Instance();
                geoAppSession.CreateNewGUID();
            }

            return base.Init();
        }

        

        public void LoggedOut()
        {

            if (session == null)
                session = DotvvmGeneric.GetSessionWrapper(Context.GetOwinContext());
            session.Clear();
            var AuthenticationManager = Context.GetOwinContext().Authentication;
            AuthenticationManager.SignOut();
            LogInLogOut = "Log In";
            LogInLogOutImage = "/images/key.png";
            Context.RedirectToUrl("/Home");
        }

        public void SignOut()
        {
            if (session == null)
                session = DotvvmGeneric.GetSessionWrapper(Context.GetOwinContext());
            
           // MapUserControl.OnSessionEnd(session);
           // ImportDatasources.OnSessionEnd(session);
            //FormsAuthentication.SignOut();
            
            //FormsAuthentication.RedirectToLoginPage();

            session.Clear();
        }

        private void SetLoggedIn(ApplicationUser thisUser)
        {
            if (session == null)
                session = DotvvmGeneric.GetSessionWrapper(Context.GetOwinContext());
            isDigitisor = userManager.IsInRole(thisUser.Id, "Digitisor") || userManager.IsInRole(thisUser.Id, "Admin");
            isAdmin = userManager.IsInRole(thisUser.Id, "Admin");
            isSuperAdmin = userManager.IsInRole(thisUser.Id, "SuperAdmin");
            UserName = thisUser.UserName.Split('_')[0];
            GeoAppUser currentUser = new GeoAppUser();
            if (currentUser != null)
            {
                if (currentUser.WorkspaceUserRoles != null)
                {
                    if (currentUser.WorkspaceUserRoles.Count() < 1)
                    {
                        LoggedOut();

                        session["UserIsDeleted"] = "Yes";

                        Context.RedirectToUrl(session["BaseURL"].ToString() + "Account/ILogin");
                    }
                }

                if (currentUser.UserDetails != null)
                {
                    bool isAllowed = bl.IsUserAllowed(currentUser, Context.HttpContext.Request.Url.ToString());
                    if (!isAllowed)
                        Context.RedirectToUrl(session["BaseURL"].ToString() + "PricePlan.aspx");

                    (FreeTrialDaysUrl,FreeTrialDaysMsg) = bl.SetLoginMessage(currentUser, session);
                    LogInLogOut = "Log Out";
                    LogInLogOutImage = "/images/Logout.png";

                    // setUserLoginLabel();
                }
            }
        }

        public void LogInOrLogOut()
        {
           if (LogInLogOut.ToLower().Contains("out"))
                LoggedOut();
            else
                Context.RedirectToUrl("/Account/Ilogin.aspx");
        }




    }
}

