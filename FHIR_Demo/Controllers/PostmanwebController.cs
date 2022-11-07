using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
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
    public class PostmanwebController : Controller
    {
        // GET: Postmanweb
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
        public async Task<dynamic> Getpostman_Block(string url, string fun, string TokenValue, string databundlejson,string Username,string Password)
        {
            if (Username != "" && Password != "" && TokenValue != "")
            {
                //var error = "Token和帳號密碼請則一!!";
                return Json(999);
            }
            else if (TokenValue!="")
            {
                if (fun == "GET")
                {
                    ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    HttpClient client = new HttpClient();
                    //client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", TokenValue); //token寫法
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TokenValue);
                    var response = await client.GetAsync(url);
                    if (response.Content.ReadAsStringAsync().Result != "")
                    {
                        var result = response.Content.ReadAsStringAsync().Result;
                        return result;
                    }
                    else
                    {
                        var result = response.StatusCode;
                        return result;
                    }
                    //return JsonConvert.DeserializeObject<omnicarejson.vivo>(result);
                }
                else if (fun == "POST")
                {
                    var data = new StringContent(databundlejson, Encoding.UTF8, "application/json");
                    ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    HttpClient client = new HttpClient();
                    //client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", TokenValue); //token寫法
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TokenValue);
                    //POST資料
                    var response = await client.PostAsync(url, data);
                    if (response.Content.ReadAsStringAsync().Result != "")
                    {
                        var result = response.Content.ReadAsStringAsync().Result;
                        return result;
                    }
                    else
                    {
                        var result = response.StatusCode;
                        return result;
                    }
                    //return JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result);
                }
                else if (fun == "PUT")
                {
                    var data = new StringContent(databundlejson, Encoding.UTF8, "application/json");
                    ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    HttpClient client = new HttpClient();
                    //client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", TokenValue); //token寫法
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TokenValue);
                    //PUT資料
                    var response = await client.PutAsync(url, data);
                    if (response.Content.ReadAsStringAsync().Result != "")
                    {
                        var result = response.Content.ReadAsStringAsync().Result;
                        return result;
                    }
                    else
                    {
                        var result = response.StatusCode;
                        return result;
                    }
                    //return JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result);
                }
                else
                {//刪除
                    ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    HttpClient client = new HttpClient();
                    //client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", TokenValue); //token寫法
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TokenValue);
                    var response = await client.DeleteAsync(url);
                    if (response.Content.ReadAsStringAsync().Result != "")
                    {
                        var result = response.Content.ReadAsStringAsync().Result;
                        return result;
                    }
                    else
                    {
                        var result = response.StatusCode;
                        return result;
                    }
                    //return JsonConvert.DeserializeObject<omnicarejson.vivo>(result);
                }
            }
            else if(Username!=""&& Password!="")
            {
                if (fun == "GET")
                {
                    var data = new StringContent(databundlejson, Encoding.UTF8, "application/json");
                    ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    HttpClient client = new HttpClient();

                    //specify to use TLS 1.2 as default connection
                    var byteArray = Encoding.ASCII.GetBytes($"{Username}:{Password}");
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

                    //POST資料
                    var response = await client.GetAsync(url);
                    if (response.Content.ReadAsStringAsync().Result != "")
                    {
                        var result = response.Content.ReadAsStringAsync().Result;
                        return result;
                    }
                    else
                    {
                        var result = response.StatusCode;
                        return result;
                    }
                    //return JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result);
                }
                else if (fun == "POST")
                {
                    var data = new StringContent(databundlejson, Encoding.UTF8, "application/json");
                    ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    HttpClient client = new HttpClient();

                    //specify to use TLS 1.2 as default connection
                    var byteArray = Encoding.ASCII.GetBytes($"{Username}:{Password}");
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

                    //POST資料
                    var response = await client.PostAsync(url, data);
                    if (response.Content.ReadAsStringAsync().Result != "")
                    {
                        var result = response.Content.ReadAsStringAsync().Result;
                        return result;
                    }
                    else
                    {
                        var result = response.StatusCode;
                        return result;
                    }
                    //return JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result);
                }
                else if (fun == "PUT")
                {
                    var data = new StringContent(databundlejson, Encoding.UTF8, "application/json");
                    ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    HttpClient client = new HttpClient();

                    //specify to use TLS 1.2 as default connection
                    var byteArray = Encoding.ASCII.GetBytes($"{Username}:{Password}");
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

                    //PUT資料
                    var response = await client.PutAsync(url, data);
                    if(response.Content.ReadAsStringAsync().Result!="")
                    {
                        var result = response.Content.ReadAsStringAsync().Result;
                        return result;
                    }
                    else
                    {
                        var result = response.StatusCode;
                        return result;
                    }
                    //var result = response.Content.ReadAsStringAsync().Result;
                    
                    //return JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result);
                }
                else
                {//刪除
                    var data = new StringContent(databundlejson, Encoding.UTF8, "application/json");
                    ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    HttpClient client = new HttpClient();

                    //specify to use TLS 1.2 as default connection
                    var byteArray = Encoding.ASCII.GetBytes($"{Username}:{Password}");
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

                    //POST資料
                    var response = await client.DeleteAsync(url);
                    if (response.Content.ReadAsStringAsync().Result != "")
                    {
                        var result = response.Content.ReadAsStringAsync().Result;
                        return result;
                    }
                    else
                    {
                        var result = response.StatusCode;
                        return result;
                    }
                    //return JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result);
                }
            }
            else
            {
                if (fun == "GET")
                {
                    ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    HttpClient client = new HttpClient();
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", TokenValue); //token寫法
                    var response = await client.GetAsync(url);
                    if (response.Content.ReadAsStringAsync().Result != "")
                    {
                        var result = response.Content.ReadAsStringAsync().Result;
                        return result;
                    }
                    else
                    {
                        var result = response.StatusCode;
                        return result;
                    }
                    //return JsonConvert.DeserializeObject<omnicarejson.vivo>(result);
                }
                else if (fun == "POST")
                {
                    //var json = JsonConvert.SerializeObject(Post_data);
                    var data = new StringContent(databundlejson, Encoding.UTF8, "application/json");
                    ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    HttpClient client = new HttpClient();
                    //POST資料
                    var response = await client.PostAsync(url, data);
                    if (response.Content.ReadAsStringAsync().Result != "")
                    {
                        var result = response.Content.ReadAsStringAsync().Result;
                        return result;
                    }
                    else
                    {
                        var result = response.StatusCode;
                        return result;
                    }
                    //return JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result);
                }
                else if (fun == "PUT")
                {
                    //var json = JsonConvert.SerializeObject(Post_data);
                    var data = new StringContent(databundlejson, Encoding.UTF8, "application/json");
                    ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    HttpClient client = new HttpClient();
                    //PUT資料
                    var response = await client.PutAsync(url, data);
                    if (response.Content.ReadAsStringAsync().Result != "")
                    {
                        var result = response.Content.ReadAsStringAsync().Result;
                        return result;
                    }
                    else
                    {
                        var result = response.StatusCode;
                        return result;
                    }
                    //return JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result);
                }
                else
                {//刪除
                    ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    HttpClient client = new HttpClient();
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", TokenValue); //token寫法
                    var response = await client.DeleteAsync(url);
                    if (response.Content.ReadAsStringAsync().Result != "")
                    {
                        var result = response.Content.ReadAsStringAsync().Result;
                        return result;
                    }
                    else
                    {
                        var result = response.StatusCode;
                        return result;
                    }
                    //return JsonConvert.DeserializeObject<omnicarejson.vivo>(result);
                }
            }



        }
    }
}