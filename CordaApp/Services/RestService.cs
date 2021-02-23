using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace CordaApp.Services
{
    public class RestService
    {
        public string httpRequestService()
        {
            string result;

            var httpWebRequest = (HttpWebRequest)WebRequest.Create("http://40.117.224.114:8080/payStates/");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "GET";

            var httpResponseQR = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponseQR.GetResponseStream()))
            {
                var resultQR = streamReader.ReadToEnd();
                result = resultQR;
            }

            return result;

        }

        public async Task<string> httpReqService(List<KeyValuePair<string, string>> parameters)
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri("http://40.117.224.114:8080");
            var request = new HttpRequestMessage(HttpMethod.Post, "/startIssuePay");

            request.Content = new FormUrlEncodedContent(parameters);
            var response = await client.SendAsync(request);

            return await response.Content.ReadAsStringAsync();
        }

    }
}
