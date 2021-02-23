using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CordaApp.Models;
using Newtonsoft.Json.Linq;
using CordaApp.DatabaseClass;
using CordaApp.Services;
using Newtonsoft.Json;
using System.Collections;
using System.Globalization;

namespace CordaApp.Controllers
{
    public class HomeController : Controller
    {
        private RestService cordaRestService;
        private readonly ILogger<HomeController> _logger;
        SqlService sqlService = new SqlService();

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
            cordaRestService = new RestService();
        }

        public IActionResult Index()
        {
            // Get Fund List
            JObject json = new JObject();
            JArray fundList = sqlService.AssetTransactionGet("Create Fund", json);

            // Get Investor List
            JArray invList = sqlService.AssetTransactionGet("Create Investor", json);

            ViewBag.fundList = fundList;
            ViewBag.invList = invList;

            return View();
        }
        public IActionResult Distributions()
        {
            // Get Fund List
            JObject json = new JObject();
            JArray fundList = sqlService.AssetTransactionGet("Create Fund", json);
            ViewBag.fundList = fundList;

            string responseStr = cordaRestService.httpRequestService();
            JArray cordaArr = (JArray)JsonConvert.DeserializeObject(responseStr);

            List<string> monthList = new List<string>();
            List<double> amountList = new List<double>();
            List<string> distList = new List<string>();
            List<double> fundRateList = new List<double>();

            foreach (var cordaItem in cordaArr)
            {
                string cordaMonth = cordaItem["state"]["data"]["month"].ToString();
                string distAmount = cordaItem["state"]["data"]["distribution"].ToString();
                if (!monthList.Contains(cordaMonth))
                {
                    double rate = 0;
                    foreach (var item in fundList)
                    {
                        double fundRate = double.Parse(item["distributionrate"].ToString());
                        json = new JObject();
                        json["fundid"] = item["fundid"];

                        string fundId = item["fundid"].ToString();

                        JArray subscriberList = sqlService.AssetTransactionGet("Create Subscriber", json);
                        double totalAmount = 0;
                        double totalFundAmount = 0;
                        foreach (var subsItem in subscriberList)
                        {
                            if (subsItem["investmentamount"] != null)
                            {
                                double subInvAmount = double.Parse(subsItem["investmentamount"].ToString());
                                totalFundAmount += subInvAmount;
                                rate = totalFundAmount * fundRate / 100;

                                string invName = "O=" + subsItem["investorname"].ToString() + ", L = New York, C = US";
                                invName = invName.Replace(" ", null);

                                foreach (var cItem in cordaArr)
                                {
                                    string owner = cItem["state"]["data"]["owner"].ToString();
                                    string cMonth = cItem["state"]["data"]["month"].ToString();
                                    string cFundId = cItem["state"]["data"]["fundid"].ToString();
                                    owner = owner.Replace(" ", null);
                                    if (invName == owner && cMonth == cordaMonth && cFundId == fundId)
                                    {
                                        totalAmount += Convert.ToDouble(cItem["state"]["data"]["amount"]);
                                    }

                                }


                            }

                        }
                        amountList.Add(totalAmount);
                        fundRateList.Add(rate);



                    }

                    monthList.Add(cordaMonth);
                    distList.Add(distAmount);
                }

            }



            ViewBag.monthList = monthList;
            ViewBag.amountList = amountList;
            ViewBag.distList = distList;
            ViewBag.fundRateList = fundRateList;

            return View();


        }

        public JsonResult AddFund(string fundName, string distRate)
        {
            JObject json = new JObject();
            json["fundname"] = fundName;
            json["distributionrate"] = distRate;

            sqlService.AssetTransactionModify("spc_AssetTransactionCreate", "Create Fund", json);

            return Json(new { success = true });
        }

        public JsonResult DeleteFund(int fundId)
        {
            JObject json = new JObject();
            json["fundid"] = fundId;

            sqlService.AssetTransactionModify("spc_AssetTransactionDelete", "Create Fund", json);
            return Json(new { success = true });
        }
        public JsonResult AddInv(string invName)
        {
            JObject json = new JObject();
            json["investorname"] = invName;

            sqlService.AssetTransactionModify("spc_AssetTransactionCreate", "Create Investor", json);
            return Json(new { success = true });
        }
        public JsonResult DeleteInv(int invId)
        {
            JObject json = new JObject();
            json["userid"] = invId;

            sqlService.AssetTransactionModify("spc_AssetTransactionDelete", "Create Investor", json);
            return Json(new { success = true });
        }
        public JsonResult AddSubscriber(int invId, int fundId, int subsAmount, string fundName, string subsName)
        {
            JObject json = new JObject();
            json["userid"] = invId;
            json["fundid"] = fundId;
            json["investmentamount"] = subsAmount;
            json["fundname"] = fundName;
            json["investorname"] = subsName;

            sqlService.AssetTransactionModify("spc_AssetTransactionCreate", "Create Subscriber", json);
            return Json(new { success = true });
        }
        public JsonResult DeleteSubs(int fundId, int invId)
        {
            JObject json = new JObject();
            json["userid"] = invId;
            json["fundid"] = fundId;

            sqlService.AssetTransactionModify("spc_AssetTransactionDelete", "Create Subscriber", json);
            return Json(new { success = true });
        }
        public JsonResult FundOrder(int fundId, int fundOr, string fundName, int fundDist)
        {
            JObject json = new JObject();
            json["fundid"] = fundId;
            json["fundname"] = fundName;
            json["fundorder"] = fundOr;
            json["distributionrate"] = fundDist;

            sqlService.AssetTransactionModify("spc_AssetTransactionUpdate", "Create Fund", json);
            return Json(new { success = true });
        }

        public JsonResult GetSubscribers(int fundId)
        {
            JObject json = new JObject();
            json["fundid"] = fundId;

            JArray subscriberList = sqlService.AssetTransactionGet("Create Subscriber", json);

            return Json(new { list = subscriberList.ToString() });
        }

        public JsonResult DistGetSubscribers(int fundId, string month)
        {
            JObject json = new JObject();
            json["fundid"] = fundId;

            JArray subscriberList = sqlService.AssetTransactionGet("Create Subscriber", json);

            string responseStr = cordaRestService.httpRequestService();
            JArray cordaArr = (JArray)JsonConvert.DeserializeObject(responseStr);

            JArray cordaRateList = new JArray();
            List<Investor> invList = new List<Investor>();

            foreach (var subscriber in subscriberList)
            {
                if (subscriber["investorname"] != null)
                {
                    string invName = "O=" + subscriber["investorname"].ToString() + ", L = New York, C = US";
                    invName = invName.Replace(" ", null);
                    double invAmount = 0;

                    foreach (var cordaItem in cordaArr)
                    {
                        string owner = cordaItem["state"]["data"]["owner"].ToString();
                        string cordaMonth = cordaItem["state"]["data"]["month"].ToString();
                        string cordaFundId = cordaItem["state"]["data"]["fundid"].ToString();
                        owner = owner.Replace(" ", null);
                        if (invName == owner && month == cordaMonth && fundId.ToString() == cordaFundId)
                        {
                            invAmount += Convert.ToDouble(cordaItem["state"]["data"]["amount"]);
                        }

                    }

                    Investor inv = new Investor
                    {
                        investorName = subscriber["investorname"].ToString(),
                        investorAmount = invAmount
                    };
                    invList.Add(inv);
                }

            }

            return Json(new { list = invList });
        }

        public async Task<JsonResult> DistRateList(string distAmount, int distMonth)
        {
            // Fund List
            JObject json = new JObject();
            JArray fundList = sqlService.AssetTransactionGet("Create Fund", json);

            double newDistAmount = double.Parse(distAmount, CultureInfo.InvariantCulture);
            double DistAmount = newDistAmount;
            while (newDistAmount > 0)
            {
                foreach (var item in fundList)
                {
                    double fundRate = double.Parse(item["distributionrate"].ToString());
                    json = new JObject();
                    json["fundid"] = item["fundid"];

                    JArray subscriberList = sqlService.AssetTransactionGet("Create Subscriber", json);

                    foreach (var subsItem in subscriberList)
                    {
                        string fundId = item["fundid"].ToString();

                        if (subsItem["investmentamount"] != null)
                        {
                            double subInvAmount = double.Parse(subsItem["investmentamount"].ToString());
                            double totalInvAmount = double.Parse(subsItem["totalinvestmentamount"].ToString());
                            double rate = ((totalInvAmount * fundRate) / 100) * (subInvAmount / totalInvAmount);

                            if (rate > newDistAmount)
                                rate = newDistAmount;
                            double cordaRate = (rate / DistAmount) * 100;
                            newDistAmount -= rate;

                            string owner = "O=" + subsItem["investorname"].ToString() + ",L=New York,C=US";
                            //string owner = "O=GroupB,L=New York,C=US";

                            if (rate > 0)
                            {
                                var reqValues = new List<KeyValuePair<string, string>>();
                                reqValues.Add(new KeyValuePair<string, string>("payName", "Dollar"));
                                reqValues.Add(new KeyValuePair<string, string>("amount", distAmount));
                                reqValues.Add(new KeyValuePair<string, string>("owner", owner));
                                reqValues.Add(new KeyValuePair<string, string>("rate", Math.Round(cordaRate, 2).ToString().Replace(",",".")));
                                reqValues.Add(new KeyValuePair<string, string>("month", distMonth.ToString()));
                                reqValues.Add(new KeyValuePair<string, string>("distribution", distAmount));
                                reqValues.Add(new KeyValuePair<string, string>("fundid", fundId));

                                string response = await cordaRestService.httpReqService(reqValues);
                            }


                        }


                    }

                    if (newDistAmount <= 0)
                        break;

                }
            }



            return Json(new { success = true });
        }



    }
}
