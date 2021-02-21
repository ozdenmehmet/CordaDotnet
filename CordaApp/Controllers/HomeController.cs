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
using CordaApp.CustomClass;

namespace CordaApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        SqlService sqlService = new SqlService();
        NodeService nodeService;

        public HomeController(ILogger<HomeController> logger)
        {
            nodeService = new NodeService();
            _logger = logger;
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
            return View();
        }

        public JsonResult AddFund(string fundName,string distRate) {
            JObject json = new JObject();
            json["fundname"] = fundName;
            json["distributionrate"] = distRate;

            sqlService.AssetTransactionModify("spc_AssetTransactionCreate", "Create Fund",json);

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

            //Create Corda Node
            nodeService.NodeCreate(invName);

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



    }
}
