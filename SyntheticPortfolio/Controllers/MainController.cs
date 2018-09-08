using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace SyntheticPortfolio.Controllers
{
    public class MainController : Controller
    {
        #region Initialize
        private string format1 = "#,##0.00";
        private string format2 = "#,##0.0000";
        private string format3 = "#,##0.000000";
        private string format_pct = "0.0000%";
        private string DateFormat = @"MM/dd/yy HH:mm";
        private string DateFormat2 = @"MM/dd/yyyy";
        private string ApiUrl;

        public MainController()
        {
            ApiUrl = ConfigurationManager.AppSettings["APIURL"].ToString();
        }
        #endregion
        #region WebPage
        public ActionResult Index()
        {
            return View();
        }
        #endregion

        #region DataServiceRequest
        private string DownloadData(string controller)
        {
            string result = string.Empty;
            //var wic = ImpersonateUser();
            using (var client = new WebClient { UseDefaultCredentials = true })
            {
                result = client.DownloadString(ApiUrl + controller);
            };
            //wic.Undo();
            //wic.Dispose();
            //wic = null;
            return result;
        }
        #endregion
    }
}