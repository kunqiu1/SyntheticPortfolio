using IBApi;
using Newtonsoft.Json;
using SyntheticPortfolio.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WVAPIDataModels;

namespace SyntheticPortfolio.Controllers
{
    [CheckAuthorization]
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
        private int IBPort;
        private int MDPort;
        private int IBId;
        private int MDId;
        private List<MktData> MktSummaryTickers;
        private bool m_isconnect { get; set; }
        public MainController()
        {
            string ApiUrl = ConfigurationManager.AppSettings["APIURL"].ToString();
            DataServiceAPI.Initialize(ApiUrl);
            IBPort = Convert.ToInt32(ConfigurationManager.AppSettings["IBPort"]);
            MDPort = Convert.ToInt32(ConfigurationManager.AppSettings["MDPort"]);
            IBId = Convert.ToInt32(ConfigurationManager.AppSettings["IBId"]);
            MDId = Convert.ToInt32(ConfigurationManager.AppSettings["MDId"]);
            MktSummaryTickers = new List<MktData>();
        }
        public void RefreshData()
        {
            m_isconnect = IsConnected() ? true : false;
            if (m_isconnect)
            {
                var acct = DataServiceAPI.DownloadData("IB/Account");
                PortfolioData.AccountData = JsonConvert.DeserializeObject<IEnumerable<IBAccountModel>>(acct);
                var port = DataServiceAPI.DownloadData("IB/Portfolio");
                PortfolioData.Portfolio = JsonConvert.DeserializeObject<IEnumerable<IBPortfolioModel>>(port);
                var cashstart = DataServiceAPI.DownloadData("IB/CashBalanceStart");
                PortfolioData.AUMSinceIncp = Convert.ToDouble(cashstart);
                var strats = DataServiceAPI.DownloadData("IB/Strategy");
                PortfolioData.AllAvailableStrategies = JsonConvert.DeserializeObject<IEnumerable<IBStrategy>>(strats).Select(x => x.StrategyName).ToList();
                var mktsummary = DataServiceAPI.DownloadData("MD/GetMarketSummary");
                MktSummaryTickers = JsonConvert.DeserializeObject<IEnumerable<MktData>>(mktsummary).ToList();

            }
        }
        private bool IsConnected()
        {
            return Convert.ToBoolean(DataServiceAPI.DownloadData("IB/IsConnected"));
        }

        #endregion


        #region WebPage
        public ActionResult DashBoard()
        {
            RefreshData();
            ViewBag.IsConnect = m_isconnect;
            if (m_isconnect)
            {
                ViewBag.summaryData = PortfolioData.GetAccountSummary();
                ViewBag.portfolioStrategy = PortfolioData.GetPortfolioByStrategy();
                ViewBag.MktSummaryData = MktSummaryTickers;
                ViewBag.NumFormat = format0;
                ViewBag.PctFormat = format_pct0;
            }
            return View();
        }

        public ActionResult Index()
        {
            RefreshData();
            ViewBag.IsConnect = m_isconnect;
            if (m_isconnect)
            {
                ViewBag.summaryData = PortfolioData.GetAccountSummary();
                ViewBag.portfolioData = PortfolioData.GetPortfolioBySecType();
                ViewBag.MktSummaryData = MktSummaryTickers;
                ViewBag.NumFormat = format0;
                ViewBag.NumFormat1 = format1;
                ViewBag.PctFormat = format_pct0;
            }
            return View();
        }
        public ActionResult StrategyPage()
        {
            RefreshData();
            ViewBag.IsConnect = m_isconnect;
            ViewBag.NumFormat = format0;
            ViewBag.NumFormat1 = format1;
            ViewBag.PctFormat = format_pct0;

            if (m_isconnect)
            {
                ViewBag.summaryData = PortfolioData.GetAccountSummary();
                ViewBag.portfolioData = PortfolioData.GetPortfolioByStrategy();
            }
            return View("StrategyPage");
        }



        #endregion


        #region get
        [HttpGet]
        public ActionResult LoginAccount()
        {
            DataServiceAPI.DownloadData($"MD/login/{MDPort}/{MDId}");
            DataServiceAPI.DownloadData($"IB/login/{IBPort}/{IBId}");
            return Json("Request sent", JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult LogoutAccount()
        {
            DataServiceAPI.DownloadData("IB/logout");
            DataServiceAPI.DownloadData("MD/logout");
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

            var result = PortfolioData.Portfolio.Where(x => x.tickerName == tag).Select(x => x.OptionLegs);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult GetTradeDetail(string ticker)
        {
            var port = PortfolioData.Portfolio.Where(x => x.tickerName == ticker).First();

            dynamic result = new ExpandoObject();
            result.last = port.marketPrice.ToString(format1);
            result.bid = port.Bid.ToString(format1);
            result.ask = port.Ask.ToString(format1);
            result.DailyPL = port.DailyPNL.ToString(format1);
            result.UnrlzPL = port.unrealizedPNL.ToString(format1);
            result.RlzPL = port.realizedPNL.ToString(format1);
            result.Position = port.position;
            result.MarketValue = port.marketValue.ToString(format1); ;
            result.CostBasis = port.premium.ToString(format1);
            result.LongShort = port.longShortRatio;
            result.Duration = port.Duration.ToString(format1);
            result.Yield = port.DividendsYield.ToString(format_pct0);
            result.DeltaPct = port.Delta1Pct.ToString(format1);
            result.DeltaDollar = port.DeltaDollar.ToString(format1);
            return Json(
                new
                {
                    result.last,
                    result.bid,
                    result.ask,
                    result.DailyPL,
                    result.UnrlzPL,
                    result.RlzPL,
                    result.Position,
                    result.MarketValue,
                    result.CostBasis,
                    result.LongShort,
                    result.Duration,
                    result.Yield,
                    result.DeltaPct,
                    result.DeltaDollar,
                },
                    JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult GetOptionDetail(string ticker)
        {
            var port = PortfolioData.Portfolio.Where(x => x.tickerName == ticker).First();
            var optionlegs = port.OptionLegs;
            return Json(optionlegs, JsonRequestBehavior.AllowGet);
        }
        #endregion


        #region POST
        [HttpPost]
        public HttpStatusCodeResult UpdateSecurity(IBstrategyMapping input)
        {
            DataServiceAPI.UploadData(input, "IB/UpdateStrategy");
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }
        #endregion
    }
}