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
                return Portfolio.Sum(x => x.DailyPNL);
            }
        }


        public static double AUMSinceIncp { get; set; }
        public static IEnumerable<IBAccountModel> AccountData { get; set; }
        public static IEnumerable<IBPortfolioModel> Portfolio { get; set; }
        public static IEnumerable<string> AllAvailableStrategies { get; set; }


        //FUnction (Others)
        //Function (Portfolio)
        public static Dictionary<string, string> GetAccountSummary()
        {
            Dictionary<string, string> summaryData = new Dictionary<string, string>();
            summaryData.Add("AUM",
                (AUM * AdjFactor).ToString(format0));
            //summaryData.Add("CuSh",
            //    (PortfolioData.ExcessLiquidty * AdjFactor).ToString(format0));
            //summaryData.Add("LVG",
            //    (PortfolioData.LeverageRatio).ToString(format_pct0));
            summaryData.Add("Init Margin",
                (InitialMargin * AdjFactor).ToString(format0));
            summaryData.Add("Incpt P&L",
                ((AUM - AUMSinceIncp) * AdjFactor).ToString(format0));
            summaryData.Add("Incpt Rtn",
                ((AUM / AUMSinceIncp - 1)).ToString(format_pct0));
            summaryData.Add("Unrlz P&L",
                (UnPL * AdjFactor).ToString(format0));
            summaryData.Add("Daily P&L",
                (DailyPL).ToString(format0));
            summaryData.Add("Daily Rtn",
                (DailyPL / AUM).ToString(format_pct0));

            return summaryData;
        }
        public static Dictionary<PerformanceSummary, IEnumerable<IBPortfolioModel>> GetPortfolioByStrategy()
        {
            IEnumerable<string> AllTypes = Portfolio.Select(x => x.strategyName).Distinct().OrderBy(x=>x);
            Dictionary<PerformanceSummary, IEnumerable<IBPortfolioModel>> portfolioData = new Dictionary<PerformanceSummary, IEnumerable<IBPortfolioModel>>();
            foreach (var item in AllTypes)
            {
                var port = Portfolio.Where(x => x.strategyName == item);
                var summary = getPerformanceSummary(port);
                summary.PortfolioType = item;
                portfolioData.Add(summary, port);
            }
            var Summary = getPerformanceSummary(Portfolio);
            Summary.PortfolioType = "zTotal";
            portfolioData.Add(Summary, Portfolio);
            return portfolioData;
        }

        public static Dictionary<PerformanceSummary, IEnumerable<IBPortfolioModel>> GetPortfolioBySecType()
        {
            IEnumerable<string> AllTypes = Portfolio.OrderBy(x => x.secType).Select(x => x.secType).Distinct();
            Dictionary<PerformanceSummary, IEnumerable<IBPortfolioModel>> portfolioData = new Dictionary<PerformanceSummary, IEnumerable<IBPortfolioModel>>();
            foreach (var item in AllTypes)
            {
                var port = Portfolio.Where(x => x.secType == item);
                var summary = getPerformanceSummary(port);
                summary.PortfolioType = item;
                portfolioData.Add(summary, port);
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
            summary.Delta1Pct = port.Select(x => x.Delta1Pct).Sum();
            summary.DeltaPoint = port.Select(x => x.DeltaPoint).Sum();
            summary.DeltaDollar = port.Select(x => x.DeltaDollar).Sum();
            summary.Gamma = port.Select(x => x.Gamma).Sum();
            summary.Vega = port.Select(x => x.Vega).Sum();
            summary.Theta = port.Select(x => x.Theta).Sum();
            summary.UnrealizedPL = port.Select(x => x.unrealizedPNL).Sum();
            summary.RealizedPL = port.Select(x => x.realizedPNL).Sum();
            summary.DailyiPL = port.Select(x => x.DailyPNL).Sum();
            summary.Duration = port.Select(x => x.Duration * x.marketValue / summary.MarketValue).Sum();
            summary.Yield = port.Select(x => x.DividendsYield * x.marketValue / summary.MarketValue).Sum();
            summary.DV01 = port.Select(x => x.DV01).Sum();
            summary.AccruedDvd = port.Select(x => x.DividendsAccrued).Sum();
            summary.AnnDvd = port.Select(x => x.marketValue * x.DividendsYield).Sum();
            return summary;
        }

    }
    public class PerformanceSummary
    {
        public string PortfolioType { get; set; }
        public int NumSecurities { get; set; }
        public double MarketValue { get; set; }
        public double Premium { get; set; }
        public double DailyiPL { get; set; }
        public double UnrealizedPL { get; set; }
        public double RealizedPL { get; set; }
        public double MarketValuePct { get; set; }
        public double Delta1Pct { get; set; }
        public double DeltaPoint { get; set; }
        public double DeltaDollar { get; set; }
        public double Gamma { get; set; }
        public double Theta { get; set; }
        public double Vega { get; set; }
        public double Duration { get; set; }
        public double Yield { get; set; }
        public double DV01 { get; set; }
        public double AccruedDvd { get; set; }
        public double AnnDvd { get; set; }


    }
}