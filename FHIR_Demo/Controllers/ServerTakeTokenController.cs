using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace FHIR_Demo.Controllers
{
    public class ServerTakeTokenController : Controller
    {
        // GET: ServerTakeToken(聯測取Token)
        public ActionResult Index()
        {

            return View();
        }
        [HttpPost]
        public ActionResult Index(string url, string fun, string TokenValue, string databundlejson, string Username, string Password)
        {

            return View();
        }

        //Get資料
        [HttpPost]
        public async Task<dynamic> Getpostman_Block(string url, string fun)
        {
            // 設定要提交的Body參數
            var formData = new Dictionary<string, string>
            {
                { "grant_type", "client_credentials" },
                { "client_id", "fhir" },
                { "client_secret", "F1lJHPZASnXn0eg7ePJy1vSC4kaKiXkM" }
            };


            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            HttpClient client = new HttpClient();
            //POST資料
            //var data = new StringContent("", Encoding.UTF8, "application/json");

            // 建立要傳送的內容
            var content = new FormUrlEncodedContent(formData);
            var response = await client.PostAsync(url, content);

            if (response.Content.ReadAsStringAsync().Result != "")
            {
                var result = response.Content.ReadAsStringAsync().Result;
                //var responseContent = await response.Content.ReadAsStringAsync();
                return result;
            }
            else
            {
                var result = response.StatusCode;
                return result;
            }
            
        }
    }
}