using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.ViewModel;
using GeoAppBuilder.BusinessLayer;
using GeoAppBuilder.Classes;
using GeoAppBuilder.Models;
using Microsoft.AspNet.Identity;

namespace GeoAppBuilder.ViewModels
{
    public class DashboardViewModel : SiteViewModel
    {
        GeneralBL bl = new GeneralBL();
        private const int defaultPageSizeRecent = 4;
        private const int defaultPageSize = 12;

        [Bind(Direction.ServerToClientFirstRequest)]
        public bool UserCanCreateMapsAndIsNotZero { get; set; }

        public int MapCount { get; set; }



        public List<string> SelectedMap { get; set; } = new List<string>();

        public GridViewDataSet<UserMaps> MapsRecent { get; set; } = new GridViewDataSet<UserMaps>() { PagingOptions = { PageSize = defaultPageSizeRecent } };
        public GridViewDataSet<UserMaps> MapsGrd { get; set; } = new GridViewDataSet<UserMaps>() { PagingOptions = { PageSize = defaultPageSize } };

        public List<string> SelectedMapPublic { get; set; } = new List<string>();

        public GridViewDataSet<PublicMaps> MapsGrdPublic { get; set; } = new GridViewDataSet<PublicMaps>() { PagingOptions = { PageSize = defaultPageSize } };



        //public AddMapControlViewModel AddMap { get; set; } = new AddMapControlViewModel();

        private HttpSessionStateWrapper session;

        public MapListControlViewModel MapList { get; set; } 

        public DashboardViewModel()
        {
            MapList = new MapListControlViewModel(bl);
        }

        public override Task Init()
        {
            UserCanCreateMapsAndIsNotZero = UserCanCreateMaps() && GetCurrentUserWorkspaceId() != 0;
            bl.Init(Context);
            MapList.NoSelect = true;
            return base.Init();
        }



        public override Task PreRender()
        {
            if (MapsGrd.IsRefreshRequired || MapsRecent.IsRefreshRequired)
            {
                GetMapsForUser();
            }

            if (MapsGrdPublic.IsRefreshRequired)
            {
                GetPublicMaps();
            }


            //UserCanCreateMapsAndIsNotZero = UserCanCreateMaps() && GetCurrentUserWorkspaceId() != 0;
            return base.PreRender();
        }



        private void GetMapsForUser()
        {
            if (session == null)
                session = DotvvmGeneric.GetSessionWrapper(Context.GetOwinContext());

            List<UserMaps> userMaps = new List<UserMaps>();
            var userManager = new ApplicationUserManager();
            ApplicationUser thisUser = null;

            try
            {
                if (session["AzimapUserName"] != null)
                {
                    //session["AzimapUserName"] = "";
                    thisUser = userManager.FindByName(session["AzimapUserName"].ToString());
                }
                if (thisUser != null)
                {
                    bool isAdmin = userManager.IsInRole(thisUser.Id, "Admin");
                    userMaps = bl.GetMapsForUser(isAdmin, thisUser);
                }

                MapCount = userMaps.Count;
                IQueryable<UserMaps> queryable = userMaps.ToArray().AsQueryable();
                MapsRecent.LoadFromQueryable(queryable);
                MapsGrd.LoadFromQueryable(queryable);
                
            }
            catch (Exception ex)
            {
                Logger.Log("Home.GetMapsForUser()" + ex.Message.ToString());
            }
            finally
            {

            }
        }

        private void GetPublicMaps()
        {
            List<PublicMaps> publicMaps = new List<PublicMaps>();
            try
            {
                publicMaps = bl.GetPublicMaps();
                IQueryable<PublicMaps> queryable = publicMaps.ToArray().AsQueryable();
                MapsGrdPublic.LoadFromQueryable(queryable);
            }
            catch (Exception ex)
            {
                Logger.Log("Home.GetPublicMaps()" + ex.Message.ToString());
            }
            finally
            {

            }
        }

        public bool UserCanCreateMaps()
        {
            bool can = false;
            try
            {
                can = bl.UserCanCreateMaps();
            }
            catch (Exception ex)
            {
                Logger.Log("Home.UserCanCreateMaps()" + ex.Message.ToString());
            }
            finally
            {

            }
            return can;
        }

        public int GetCurrentUserWorkspaceId()
        {
            if (session == null)
                session = DotvvmGeneric.GetSessionWrapper(Context.GetOwinContext());
            GeoAppUser currentUser = new GeoAppUser();
            return bl.GetCurrentUserWorkspaceId(currentUser);
        }

        public void OpenCreateMapDialog()
        {
            //AddMap.AddMap_Displayed = true;
        }
    }
}

