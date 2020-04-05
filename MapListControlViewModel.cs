using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.ViewModel;
using GeoAppBuilder.BusinessLayer;
using GeoAppBuilder.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace GeoAppBuilder.ViewModels
{
    public class MapListControlViewModel : DotvvmViewModelBase
    {
        private GeneralBL MapList_bl;
        private const int MapList_defaultPageSize = 12;
        public int MapList_PageSize { get; } = MapList_defaultPageSize;
        public GridViewDataSet<MapListModel> MapList_MapsList { get; set; } = new GridViewDataSet<MapListModel>() { PagingOptions = { PageSize = MapList_defaultPageSize } };
        private HttpSessionStateWrapper session;

        public List<string> MapList_SelectedMap { get; set; } = new List<string>();

        public bool NoSelect { get; set; } = false;


        public int MapList_MapId { get; set; }
        public bool MapList_Displayed { get; set; } = false;
        public string MapList_MapName { get; set; } = "";
        public string MapList_MapDesc { get; set; } = "";

        public MapListControlViewModel(GeneralBL bl) {
            MapList_bl = bl;
        }

        public override Task Init()
        {
            MapList_bl.Init(Context);
            return base.Init();
        }

        public override Task PreRender()
        {
            if (MapList_MapsList.IsRefreshRequired)
            {
                GetMapsForUser();
            }

            //UserCanCreateMapsAndIsNotZero = UserCanCreateMaps() && GetCurrentUserWorkspaceId() != 0;
            return base.PreRender();
        }

        private void GetMapsForUser()
        {
            if (session == null)
                session = DotvvmGeneric.GetSessionWrapper(Context.GetOwinContext());

            List<MapListModel> userMaps = new List<MapListModel>();
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
                    userMaps = MapList_bl.SetupMapList(isAdmin, thisUser);
                }

                //MapCount = userMaps.Count;
                IQueryable<MapListModel> queryable = userMaps.ToArray().AsQueryable();
                MapList_MapsList.LoadFromQueryable(queryable);

            }
            catch (Exception ex)
            {
                Logger.Log("MapList.GetMapsForUser()" + ex.Message.ToString());
            }
            finally
            {

            }
        }

        public void OnEdit(int MapId)
        {
            int i = MapId;
        }
        public void OnEdit()
        {

        }

        public void SaveMapNameDesc()
        {

        }

        public void EnableEditMapNameDesc()
        {
            MapList_Displayed = true;
        }

        //public void NewMap1()
        //{

        //    var httpContext = DotvvmGeneric.GetHttpContext(Context.GetOwinContext());
        //    MapList_MapId = MapList_bl.SaveNewMap(MapList_MapName, MapList_MapDesc, httpContext.Session["AziMapUserName"].ToString(), httpContext);
        //    if (MapList_MapId != -1)
        //        Context.RedirectToUrl("/MemberPages/Default.aspx?MapID=" + MapList_MapId);

        //}

        //public void OpenAdvancedMap1()
        //{
        //    Context.RedirectToUrl("~/MemberPages/MapDef.aspx?mode=Advanced&mapName=" + MapList_MapName + "&mapDesc=" + MapList_MapDesc);
        //}

        

    }
}