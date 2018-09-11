using IBApi;
using Newtonsoft.Json;
using SyntheticPortfolio.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WVAPIDataModels;

namespace SyntheticPortfolio.Controllers
{
    public class MainController : Controller
    {
        #region Initialize
        private string format0 = "#,##0";
        private string format1 = "#,##0.00";
        private string format2 = "#,##0.0000";
        private string format3 = "#,##0.000000";
        private string format_pct0 = "0.00%";
        private string format_pct = "0.0000%";
        private string DateFormat = @"MM/dd/yy HH:mm";
        private string DateFormat2 = @"MM/dd/yyyy";
        private string ApiUrl;
        private int IBPort;
        private int MDPort;
        private int IBId;
        private int MDId;
        private List<int> MktSummaryTickers;
        private bool m_isconnect;
        public MainController()
        {
            ApiUrl = ConfigurationManager.AppSettings["APIURL"].ToString();
            IBPort = Convert.ToInt32(ConfigurationManager.AppSettings["IBPort"]);
            MDPort = Convert.ToInt32(ConfigurationManager.AppSettings["MDPort"]);
            IBId = Convert.ToInt32(ConfigurationManager.AppSettings["IBId"]);
            MDId = Convert.ToInt32(ConfigurationManager.AppSettings["MDId"]);
            MktSummaryTickers = new List<int>();
            List<string> temp = ConfigurationManager.AppSettings["TickersFUT"].ToString().Split(';').ToList();
            int i = 1;
            foreach (var item in temp)
            {
                List<string> temp0 = item.Split(',').ToList();
                var c = new Contract();
                c.Symbol = temp0[0];
                c.PrimaryExch = temp0[1];
                c.LastTradeDateOrContractMonth = temp0[2];
                c.ConId = Convert.ToInt32(temp0[3]);
                c.SecType = "FUT";
                PortfolioData.AddMDTickers(c);
                i++;
                MktSummaryTickers.Add(c.ConId);
            }
            List<string> temp2 = ConfigurationManager.AppSettings["TickersSTK"].ToString().Split(';').ToList();
            foreach (var item in temp2)
            {
                List<string> temp0 = item.Split(',').ToList();
                var c = new Contract();
                c.Symbol = temp0[0];
                c.LocalSymbol = temp0[0];
                c.PrimaryExch = temp0[1];
                c.ConId = Convert.ToInt32(temp0[2]);
                c.SecType = "STK";
                PortfolioData.AddMDTickers(c);
                i++;
            }
        }
        public void RefreshData()
        {
            m_isconnect = IsConnected() ? true : false;
            if (m_isconnect)
            {
                var acct = DownloadData("IB/Account");
                PortfolioData.AccountData = JsonConvert.DeserializeObject<IEnumerable<IBAccountModel>>(acct);
                var port = DownloadData("IB/Portfolio");
                PortfolioData.Portfolio = JsonConvert.DeserializeObject<IEnumerable<IBPortfolioModel>>(port);
                var cashstart = DownloadData("IB/CashBalanceStart");
                PortfolioData.AUMSinceIncp = Convert.ToDouble(cashstart);
                var strats = DownloadData("IB/Strategy");

                PortfolioData.AllAvailableStrategies = JsonConvert.DeserializeObject<IEnumerable<IBStrategy>>(strats).Select(x => x.StrategyName).ToList();
                UploadData(PortfolioData.MDTickers, "MD/StartMktReq");

                var mdTickers = DownloadData("MD/MarketData");
                PortfolioData.RefreshMDTickers(JsonConvert.DeserializeObject<IEnumerable<MktData>>(mdTickers));



            }
        }


        #endregion
        #region WebPage
        public ActionResult Index()
        {
            RefreshData();
            ViewBag.IsConnect = m_isconnect;
            if (m_isconnect)
            {
                List<MatlabContractModel> MktSummaryData = PortfolioData.MDTickers.Where(x => MktSummaryTickers.Contains(x.contract.ConId)).ToList();

                ViewBag.summaryData = PortfolioData.GetAccountSummary();
                ViewBag.portfolioData = PortfolioData.GetPortfolioBySecType();
                ViewBag.MktSummaryData = MktSummaryData;
            }
            return View();
        }
        public ActionResult StrategyPage()
        {
            RefreshData();
            ViewBag.IsConnect = m_isconnect;
            if (m_isconnect)
            {
                ViewBag.summaryData = PortfolioData.GetAccountSummary();
                ViewBag.portfolioData = PortfolioData.GetPortfolioByStrategy();
            }
            return View("StrategyPage");
        }


        #endregion
        #region DataServiceRequest
        private string DownloadData(string controller)
        {
            string result = string.Empty;
            using (var client = new WebClient { UseDefaultCredentials = true })
            {
                result = client.DownloadString(ApiUrl + controller);
            };
            return result;
        }
        private void UploadData(dynamic Data,string controller)
        {
            using (var client = new WebClient { UseDefaultCredentials = true })
            {
                client.Headers.Add(HttpRequestHeader.ContentType, "application/json; charset=utf-8");
                client.UploadString(ApiUrl + controller, JsonConvert.SerializeObject(Data));
            }
        }
        public bool IsConnected()
        {
            return Convert.ToBoolean(DownloadData("IB/IsConnected"));
        }
        #endregion


        #region get
        [HttpGet]
        public ActionResult LoginAccount()
        {
            DownloadData($"IB/login/{IBPort}/{IBId}");
            DownloadData($"MD/login/{MDPort}/{MDId}");
            return Json("Request sent", JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult LogoutAccount()
        {
            DownloadData("IB/logout");
            DownloadData("MD/logout");
            return Json("Request sent", JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult GetAvailableStrategies()
        {
            return Json(PortfolioData.AllAvailableStrategies, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult GetOptionLegs(string tag)
        {

            var result = PortfolioData.Portfolio.Where(x => x.strategyNameOption == tag).ToList();
            return Json(result,JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region POST
        [HttpPost]
        public HttpStatusCodeResult UpdateSecurity(IBstrategyMapping input)
        {
            UploadData(input, "IB/UpdateStrategy");
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }
        #endregion
    }
}