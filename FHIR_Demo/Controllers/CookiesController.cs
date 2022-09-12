using Hl7.Fhir.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FHIR_Demo.Controllers
{
    public class CookiesController
    {
        //public string FHIR_url = "https://oauth.dicom.org.tw/fhir";
        //public string FHIR_Token = "3d821661-6dc0-40b1-a8d5-2e91389255bb";
        //
        //public string FHIR_url;
        //public string FHIR_Token;
        //public string FHIR_Server;
        public string FHIR_url = "https://vtb02p.vghks.gov.tw/fhir-server/api/v4";
        public string FHIR_Token = "3d821661-6dc0-40b1-a8d5-2e91389255bb";
        public string FHIR_Server = "HAPi";

        public FhirClientSettings settings = new FhirClientSettings
        {
            Timeout = 120000,
            PreferredFormat = ResourceFormat.Json,
        };

        /// <summary>
        /// 預設Cookie
        /// </summary>
        /// <param name="httpContext"></param>
        public void Init_Cookie(HttpContextBase httpContext) 
        {
            FHIR_URL_Cookie(httpContext);
            FHIR_Token_Cookie(httpContext);
            FHIR_Server_Cookie(httpContext);
        }

        /// <summary>
        /// 取得URL的Cookie，若沒有值會初始化值
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public string FHIR_URL_Cookie(HttpContextBase httpContext)
        {
            string FHIR_URL_Cookie;
            HttpCookie Cookie = httpContext.Request.Cookies["FHIR_url"] ?? null;
            if (Cookie == null || Cookie.Value == "")
            {
                Cookie = new HttpCookie("FHIR_url", FHIR_url);
            }
            FHIR_URL_Cookie = Cookie.Value;
            Cookie.Expires = DateTime.Now.AddDays(1); //設置Cookie到期時間
            httpContext.Response.Cookies.Add(Cookie);
            return FHIR_URL_Cookie;
         }

        /// <summary>
        /// 設定URL的Cookie
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="FHIR_url_update"></param>
        /// <returns></returns>
        public string FHIR_URL_Cookie(HttpContextBase httpContext, string FHIR_url_update)
        {
            HttpCookie Cookie = httpContext.Request.Cookies["FHIR_url"];
            if (Cookie == null || Cookie.Value == "")
            {
                Cookie = new HttpCookie("FHIR_url", FHIR_url_update);
            }
            else
            {
                Cookie.Value = FHIR_url_update;
            }
            Cookie.Expires = DateTime.Now.AddDays(1); //設置Cookie到期時間
            httpContext.Response.Cookies.Add(Cookie);
            return FHIR_url_update;
        }

        /// <summary>
        /// 取得Token(或IBM帳號與密碼)的Cookie，若沒有值會初始化值
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public string FHIR_Token_Cookie(HttpContextBase httpContext)
        {
            string FHIR_Token_Cookie;
            HttpCookie Cookie = httpContext.Request.Cookies["FHIR_Token"] ?? null;
            if (Cookie == null || Cookie.Value == "")
            {
                Cookie = new HttpCookie("FHIR_Token", FHIR_Token);
            }
            FHIR_Token_Cookie = Cookie.Value;
            Cookie.Expires = DateTime.Now.AddDays(1); //設置Cookie到期時間
            httpContext.Response.Cookies.Add(Cookie);
            return FHIR_Token_Cookie;
        }

        /// <summary>
        /// 設定Token(或IBM帳號與密碼)的Cookie
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="FHIR_Token_update"></param>
        /// <returns></returns>
        public string FHIR_Token_Cookie(HttpContextBase httpContext, string FHIR_Token_update)
        {
            HttpCookie Cookie = httpContext.Request.Cookies["FHIR_Token"];
            if (Cookie == null || Cookie.Value == "")
            {
                Cookie = new HttpCookie("FHIR_Token", FHIR_Token_update);
            }
            else
            {
                Cookie.Value = FHIR_Token_update;
            }
            Cookie.Expires = DateTime.Now.AddDays(1); //設置Cookie到期時間
            httpContext.Response.Cookies.Add(Cookie);
            return FHIR_Token_update;
        }

        /// <summary>
        /// 取得Server的Cookie，若沒有值會初始化值
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public string FHIR_Server_Cookie(HttpContextBase httpContext)
        {
            string FHIR_Token_Cookie;
            HttpCookie Cookie = httpContext.Request.Cookies["FHIR_Server"] ?? null;
            if (Cookie == null || Cookie.Value == "")
            {
                Cookie = new HttpCookie("FHIR_Server", FHIR_Server);
            }
            FHIR_Token_Cookie = Cookie.Value;
            Cookie.Expires = DateTime.Now.AddDays(1); //設置Cookie到期時間
            httpContext.Response.Cookies.Add(Cookie);
            return FHIR_Token_Cookie;
        }

        /// <summary>
        /// 設定Server的Cookie
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="FHIR_Token_update"></param>
        /// <returns></returns>
        public string FHIR_Server_Cookie(HttpContextBase httpContext, string FHIR_Token_update)
        {
            HttpCookie Cookie = httpContext.Request.Cookies["FHIR_Server"];
            if (Cookie == null || Cookie.Value == "")
            {
                Cookie = new HttpCookie("FHIR_Server", FHIR_Token_update);
            }
            else
            {
                Cookie.Value = FHIR_Token_update;
            }
            Cookie.Expires = DateTime.Now.AddDays(1); //設置Cookie到期時間
            httpContext.Response.Cookies.Add(Cookie);
            return FHIR_Token_update;
        }

    }
}