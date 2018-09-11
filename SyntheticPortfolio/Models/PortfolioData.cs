using IBApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WVAPIDataModels;

namespace SyntheticPortfolio.Models
{
    public static class PortfolioData
    {
        private const double AdjFactor = 0.001;
        private const string format0 = "#,##0";
        private const string format_pct0 = "0.00%";

        public static double AUM
        {
            get
            {
                return Convert.ToDouble(AccountData.Where(x => x.key == "NetLiquidation")
                    .Select(x => x.value).FirstOrDefault());
            }
        }
        public static double Cash
        {
            get
            {
                return Convert.ToDouble(AccountData.Where(x => x.key == "TotalCashValue")
                    .Select(x => x.value).FirstOrDefault());
            }
        }
        public static double UnPL
        {
            get
            {
                return Convert.ToDouble(AccountData.Where(x => x.key == "UnrealizedPnL")
                    .Select(x => x.value).FirstOrDefault());
            }
        }
        public static double LeverageRatio
        {
            get
            {
                return Convert.ToDouble(AccountData.Where(x => x.key == "Leverage-S")
                    .Select(x => x.value).FirstOrDefault());
            }
        }
        public static double ExcessLiquidty
        {
            get
            {
                return Convert.ToDouble(AccountData.Where(x => x.key == "FullExcessLiquidity")
                    .Select(x => x.value).FirstOrDefault());
            }
        }
        public static double DividendReceivable
        {
            get
            {
                return Convert.ToDouble(AccountData.Where(x => x.key == "AccruedDividend")
                 .Select(x => x.value).FirstOrDefault());
            }
        }
        public static double InitialMargin
        {
            get
            {
                return Convert.ToDouble(AccountData.Where(x => x.key == "FullInitMarginReq")
                 .Select(x => x.value).FirstOrDefault());
            }
        }
        public static double DailyPL
        {
            get
            {
                return Portfolio.Select(x => x.DailyPNL).Sum();
            }
        }


        public static double AUMSinceIncp { get; set; }
        public static IEnumerable<IBAccountModel> AccountData { get; set; }
        private static IEnumerable<IBPortfolioModel> portfolio;



        public static IEnumerable<IBPortfolioModel> Portfolio
        {
            get
            {
                return portfolio;
            }
            set
            {
                portfolio = value;
                foreach (var item in portfolio)
                {
                    AddMDTickers(item.contract);
                }
            }
        }


        public static List<MatlabContractModel> MDTickers { get; set; }
        public static void AddMDTickers(Contract contract)
        {
            if (MDTickers == null)
            {
                MDTickers = new List<MatlabContractModel>();
            }
            if (!MDTickers.Any(x => x.contract.ConId == contract.ConId))
            {

                int i = 1;
                if (MDTickers.Count() > 0)
                {
                    i = MDTickers.Max(x => x.ReqId) + 1;
                }
                MDTickers.Add(new MatlabContractModel(contract, i));
            }
        }
        internal static void RefreshMDTickers(IEnumerable<MktData> MKTData)
        {
            foreach (var item in MKTData)
            {
                var mdticker = MDTickers.Where(x => x.contract.ConId == item.ConId).First();
                if (mdticker != null)
                {
                    if (mdticker.contract.ConId== 43652089)
                    {

                    }
                    double multiplier = 1;
                    if (mdticker.contract.Multiplier != null)
                    {
                        multiplier = Convert.ToDouble(mdticker.contract.Multiplier);
                    }
                    if (item.mktdata.Keys.Contains("lastPrice"))
                    {
                        mdticker.last = item.mktdata["lastPrice"];
                        //port.marketValue = mdticker.last*port.position* multiplier;
                    }
                    if (item.mktdata.Keys.Contains("open"))
                    {
                        mdticker.open = item.mktdata["open"];
                    }
                    if (item.mktdata.Keys.Contains("high"))
                    {
                        mdticker.high = item.mktdata["high"];
                    }
                    if (item.mktdata.Keys.Contains("low"))
                    {
                        mdticker.low = item.mktdata["low"];
                    }
                    if (item.mktdata.Keys.Contains("close"))
                    {
                        mdticker.close = item.mktdata["close"];
                    }
                    if (item.mktdata.Keys.Contains("delta"))
                    {
                        mdticker.delta = item.mktdata["delta"];
                    }
                    if (item.mktdata.Keys.Contains("gamma"))
                    {
                        mdticker.gamma = item.mktdata["gamma"];
                    }
                    if (item.mktdata.Keys.Contains("theta"))
                    {
                        mdticker.theta = item.mktdata["theta"];
                    }
                    if (item.mktdata.Keys.Contains("vega"))
                    {
                        mdticker.vega = item.mktdata["vega"];
                    }
                    if (item.mktdata.Keys.Contains("optPrice"))
                    {
                        mdticker.last = item.mktdata["optPrice"];
                    }
                    if (item.mktdata.Keys.Contains("undPrice"))
                    {
                        mdticker.lastunderlying = item.mktdata["undPrice"];
                    }
                    var port = portfolio.Where(x => x.contractID == item.ConId).FirstOrDefault();
                    if (port != null)
                    {
                        port.marketPrice = mdticker.last;
                        port.Underlying = mdticker.lastunderlying;
                        port.Delta = mdticker.delta * port.position * multiplier*mdticker.lastunderlying*0.01;
                        port.Gamma = mdticker.gamma * port.position * multiplier * mdticker.lastunderlying * 0.01;
                        port.Theta = mdticker.theta * port.position * multiplier * mdticker.lastunderlying * 0.01;
                        port.Vega = mdticker.vega * port.position * multiplier * mdticker.lastunderlying * 0.01;
                    }
                    var portOption = MDTickers.Where(x => (x.contract.SecType == "OPT" || x.contract.SecType == "FOP")
                    && x.contract.Symbol == mdticker.contract.LocalSymbol);
                    foreach (var option in portOption)
                    {
                        option.lastunderlying = mdticker.last!=0?mdticker.last:mdticker.close;
                    }
                }
            }
        }
        public static IEnumerable<string> AllAvailableStrategies { get; set; }


        //FUnction (Others)
        //Function (Portfolio)
        public static Dictionary<string, string> GetAccountSummary()
        {
            Dictionary<string, string> summaryData = new Dictionary<string, string>();
            summaryData.Add("AUM",
                (PortfolioData.AUM * AdjFactor).ToString(format0));
            summaryData.Add("CuSh",
                (PortfolioData.ExcessLiquidty * AdjFactor).ToString(format0));
            summaryData.Add("LVG",
                (PortfolioData.LeverageRatio).ToString(format_pct0));
            summaryData.Add("Incpt P&L",
                ((PortfolioData.AUM - PortfolioData.AUMSinceIncp) * AdjFactor).ToString(format0));
            summaryData.Add("Incpt Rtn",
                ((PortfolioData.AUM / PortfolioData.AUMSinceIncp - 1)).ToString(format_pct0));
            summaryData.Add("DVD Rcvble",
                (PortfolioData.DividendReceivable).ToString(format0));
            summaryData.Add("Daily P&L",
                (PortfolioData.DailyPL * AdjFactor).ToString(format0));
            summaryData.Add("Init Margin",
                (PortfolioData.InitialMargin * AdjFactor).ToString(format0));
            return summaryData;
        }
        public static Dictionary<PerformanceSummary, IEnumerable<IBPortfolioModel>> GetPortfolioByStrategy()
        {
            IEnumerable<string> AllTypes = Portfolio.Select(x => x.strategyName).Distinct();
            Dictionary<PerformanceSummary, IEnumerable<IBPortfolioModel>> portfolioData = new Dictionary<PerformanceSummary, IEnumerable<IBPortfolioModel>>();
            foreach (var item in AllTypes)
            {
                var port = Portfolio.Where(x => x.strategyName == item);
                var summary = getPerformanceSummary(port);
                summary.PortfolioType = item;
                portfolioData.Add(summary, port);
            }
            return portfolioData;
        }

        public static Dictionary<PerformanceSummary, IEnumerable<IBPortfolioModel>> GetPortfolioBySecType()
        {
            IEnumerable<string> AllTypes = Portfolio.Select(x => x.secType).Distinct();
            Dictionary<PerformanceSummary, IEnumerable<IBPortfolioModel>> portfolioData = new Dictionary<PerformanceSummary, IEnumerable<IBPortfolioModel>>();
            foreach (var item in AllTypes)
            {
                var port = Portfolio.Where(x => x.secType == item);
                var summary = getPerformanceSummary(port);
                summary.PortfolioType = item;

                if (item == "OPT" || item=="FOP")
                {
                    summary.Gamma = port.Select(x => x.Gamma).Sum();
                    summary.Theta = port.Select(x => x.Theta).Sum();
                    summary.Vega = port.Select(x => x.Vega).Sum();
                    var portOption = new List<IBPortfolioModel>();
                    var allOptionstrategy = port.Select(x => x.strategyNameOption).Distinct();
                    foreach (var item2 in allOptionstrategy)
                    {
                        var portOptiontemp = port.Where(x => x.strategyNameOption == item2);
                        var posOption = new IBPortfolioModel();
                        posOption.tickerName = portOptiontemp.Select(x => x.contract.Symbol).First();
                        posOption.strategyNameOption = item2;
                        posOption.contract = new IBApi.Contract();
                        posOption.contract.LastTradeDateOrContractMonth = portOptiontemp.Select(x => x.contract.LastTradeDateOrContractMonth).First();
                        posOption.contract.PrimaryExch = portOptiontemp.Select(x => x.contract.PrimaryExch).First();
                        posOption.marketValue = portOptiontemp.Select(x => x.marketValue).Sum();
                        posOption.averageCost = portOptiontemp.Select(x => x.averageCost).Sum();
                        posOption.premium = portOptiontemp.Select(x => x.premium).Sum();
                        posOption.unrealizedPNL = portOptiontemp.Select(x => x.unrealizedPNL).Sum();
                        posOption.realizedPNL = portOptiontemp.Select(x => x.realizedPNL).Sum();
                        posOption.DailyPNL = portOptiontemp.Select(x => x.DailyPNL).Sum();
                        posOption.Delta = portOptiontemp.Select(x => x.Delta).Sum();
                        posOption.Gamma = portOptiontemp.Select(x => x.Gamma).Sum();
                        posOption.Theta = portOptiontemp.Select(x => x.Theta).Sum();
                        posOption.Vega = portOptiontemp.Select(x => x.Vega).Sum();
                        posOption.LastUpdate = portOptiontemp.Select(x => x.LastUpdate).First();
                        posOption.Notes = portOptiontemp.Count().ToString();
                        portOption.Add(posOption);
                    }
                    portfolioData.Add(summary, portOption);
                }
                else
                {
                    portfolioData.Add(summary, port);
                }
            }
            return portfolioData;
        }
        private static PerformanceSummary getPerformanceSummary(IEnumerable<IBPortfolioModel> port)
        {
            var summary = new PerformanceSummary();
            summary.NumSecurities = port.Count();
            summary.MarketValue = port.Select(x => x.marketValue).Sum();
            summary.MarketValuePct = summary.MarketValue / Portfolio.Select(x => x.marketValue).Sum();
            summary.Premium = port.Select(x => x.premium).Sum();
            summary.Delta = port.Select(x => x.Delta).Sum();
            summary.UnrealizedPL = port.Select(x => x.unrealizedPNL).Sum();
            summary.RealizedPL = port.Select(x => x.realizedPNL).Sum();
            summary.DailyiPL = port.Select(x => x.DailyPNL).Sum();
            return summary;
        }

    }
    public class PerformanceSummary
    {
        public string PortfolioType { get; set; }
        public int NumSecurities { get; set; }
        public double MarketValue { get; set; }
        public double Premium { get; set; }
        public double Notional { get; set; }
        public double DailyiPL { get; set; }
        public double UnrealizedPL { get; set; }
        public double RealizedPL { get; set; }
        public double MarketValuePct { get; set; }
        public double Delta { get; set; }
        public double Gamma { get; set; }
        public double Theta { get; set; }
        public double Vega { get; set; }

    }
}