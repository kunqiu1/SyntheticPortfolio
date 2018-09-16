using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using Newtonsoft.Json;

namespace SyntheticPortfolio.Models
{
    public static class DataServiceAPI
    {
        private static string ApiUrl;
        public static void Initialize(string ApiURL)
        {
            ApiUrl = ApiURL;
        }
        public static string DownloadData(string controller)
        {
            try
            {
                string result = string.Empty;
                using (var client = new WebClient { UseDefaultCredentials = true })
                {
                    result = client.DownloadString(ApiUrl + controller);
                };
                return result;
            }
            catch
            {
                return "ERROR";
            }
        }
        public static void UploadData(dynamic Data, string controller)
        {
            try
            {
                using (var client = new WebClient { UseDefaultCredentials = true })
                {
                    client.Headers.Add(HttpRequestHeader.ContentType, "application/json; charset=utf-8");
                    client.UploadString(ApiUrl + controller, JsonConvert.SerializeObject(Data));
                }
            }
            catch
            {
            }
        }
    }
}