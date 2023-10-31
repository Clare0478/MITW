using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using FHIR_Demo.Models;
using Hl7.Fhir.Rest;
using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Newtonsoft.Json.Linq;
using System.Net;
using System.IdentityModel.Tokens.Jwt;

namespace FHIR_Demo.Controllers
{

    [Authorize]
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        //SMART ON FHIR
        private FHIRResourceModel db = new FHIRResourceModel();
        private CookiesController cookies = new CookiesController();

        //smart 網址參數設定
        private static string _clientId = "fhir_demo_id";//要供輸入不能寫死(後端去生成)
        private static string _authCode = "";
        private static string _clientState = "";
        private static string _redirectUrl = "https://localhost:44309/Account/HandleAuthCode";//導回頁面
        //private static string _redirectUrl = "https://nutrition.ntunhs.edu.tw/FHIR_Demo/Account/HandleAuthCode";//導回頁面
        //private static string _redirectUrl = "https://silcoet.ntunhs.edu.tw/FHIR_Demo/Account/HandleAuthCode";//導回頁面
        private static string _tokenUrl = "";   // "http://10.20.15.157:4013/v/r4/auth/token";//要取Token的URL
        private static string _fhirServerUrl = "";//要連線的伺服器
        private string token = "";



        [HttpPost]
        public ActionResult GetToken(string Fhirurl,string Clientid)
        {
            _fhirServerUrl = Fhirurl;
            //int lastSlashIndex = _fhirServerUrl.LastIndexOf('/');
            //int secondLastSlashIndex = _fhirServerUrl.LastIndexOf('/', lastSlashIndex - 1);
            //string extractedString = _fhirServerUrl.Substring(secondLastSlashIndex + 1, lastSlashIndex - secondLastSlashIndex - 1);

            //var handler = new JwtSecurityTokenHandler();
            //var token = handler.ReadJwtToken(extractedString);

            //// 檢查要求的URL裡的Clientid資料是否符合輸入
            //var claims = token.Claims;
            //string clientId = claims.FirstOrDefault(claim => claim.Type == "client_id")?.Value;

            _tokenUrl = _fhirServerUrl.Replace("fhir", "auth/token"); //要取Token的URL

            //if(Clientid != clientId)
            //{
            //    return Content("error");
            //}

            //smart(整包要授權的Url(授權網址+參數設定))
            string tokenUrl =
                $"{_fhirServerUrl.Replace("fhir", "auth/authorize")}" + //要授權的Url(跟smart on fhir取)
                $"?response_type=code" + //授權類型(code:提供授權碼)
                $"&client_id={Clientid}" +//要授權的身分id
                $"&redirect_uri={HttpUtility.UrlEncode(_redirectUrl)}" +//授權完導向的網址
                $"&system scope={HttpUtility.UrlEncode("openid fhirUser profile launch/patient patient/*.read")}" +//權限設置
                $"&state=local_state" +//防止攻擊
                $"&aud={_fhirServerUrl}";//要連線的伺服器

            //string redirectUrl = Url.Action("HandleAuthCode", "Account", null, Request.Url.Scheme);
            //return Redirect(tokenUrl);

            //避免在Https網頁中導向Http網頁時出錯
            var client = new HttpClient();
                var response = client.GetAsync(tokenUrl).Result;

                if (response.IsSuccessStatusCode)
                {
                    string content = response.Content.ReadAsStringAsync().Result;

                    //return Content(content, "text/html");
                    return Content(content, "text/plain");
                    //return Json(new { Token= token});
                }
                else
                {
                    //回傳失敗
                    return Content("error");
                }
        }

        //拿取Acess_Token
        public async Task<string> SetAuthCode(string code, string state)
        {
            _authCode = code;
            _clientState = state;

            if (!string.IsNullOrEmpty(code) && !string.IsNullOrEmpty(state))
            {
                // 組裝請求所需的參數
                Dictionary<string, string> requestValues = new Dictionary<string, string>()
                {
                    { "grant_type", "authorization_code" },//授權類型(授權碼驗證類型)
                    { "code", code },//取token需要的授權許可碼
                    { "redirect_uri", _redirectUrl },//我在授權的時候對應的URL(要對得上)
                    { "client_id", _clientId }//
                };

                using (HttpClient client = new HttpClient())
                {
                    try
                    {
                        //導入到_tokenUrl取token(用授權抓到的授權碼等參數)
                        HttpRequestMessage request = new HttpRequestMessage()
                        {
                            Method = HttpMethod.Post,
                            RequestUri = new Uri(_tokenUrl),
                            Content = new FormUrlEncodedContent(requestValues)
                        };

                        //回傳抓到的網址POST碼
                        HttpResponseMessage response = await client.SendAsync(request);

                        if (response.IsSuccessStatusCode)
                        {
                            string json = await response.Content.ReadAsStringAsync();
                            SmartResponse smartResponse = JsonSerializer.Deserialize<SmartResponse>(json);
                            token = smartResponse.AccessToken;
                        }
                        //await Task.Run(() => DoSomethingWithToken(smartResponse));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("An error occurred: " + ex.Message);
                    }
                }
            }
            else
            {
                token = "";
            }

            return token;
        }

        [AllowAnonymous]
        public async Task<ActionResult> HandleAuthCode()
        {
            string code = Request["code"];//授權碼(取Token用)
            string state = Request["state"];//防攻擊

            token = await SetAuthCode(code, state);

            // 儲存 token 於 Session
            Session["Token"] = token;
            Session["FhirServerUrl"] = _fhirServerUrl;// cookies.FHIR_URL_Cookie(HttpContext);

            //return RedirectToAction("Index", "Home");
            return Content(token, "text/plain");
        }


        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // 這不會計算為帳戶鎖定的登入失敗
            // 若要啟用密碼失敗來觸發帳戶鎖定，請變更為 shouldLockout: true
            var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "登入嘗試失試。");
                    return View(model);
            }
        }

        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // 需要使用者已透過使用者名稱/密碼或外部登入進行登入
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // 下列程式碼保護兩個因素碼不受暴力密碼破解攻擊。 
            // 如果使用者輸入不正確的代碼來表示一段指定的時間，則使用者帳戶 
            // 會有一段指定的時間遭到鎖定。 
            // 您可以在 IdentityConfig 中設定帳戶鎖定設定
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent: model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "代碼無效。");
                    return View(model);
            }
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

                    // 如需如何進行帳戶確認及密碼重設的詳細資訊，請前往 https://go.microsoft.com/fwlink/?LinkID=320771
                    // 傳送包含此連結的電子郵件
                    // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    // await UserManager.SendEmailAsync(user.Id, "確認您的帳戶", "請按一下此連結確認您的帳戶 <a href=\"" + callbackUrl + "\">這裏</a>");

                    return RedirectToAction("Index", "Home");
                }
                AddErrors(result);
            }

            // 如果執行到這裡，發生某項失敗，則重新顯示表單
            return View(model);
        }

        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(model.Email);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // 不顯示使用者不存在或未受確認
                    return View("ForgotPasswordConfirmation");
                }

                // 如需如何進行帳戶確認及密碼重設的詳細資訊，請前往 https://go.microsoft.com/fwlink/?LinkID=320771
                // 傳送包含此連結的電子郵件
                // string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                // var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);		
                // await UserManager.SendEmailAsync(user.Id, "重設密碼", "請按 <a href=\"" + callbackUrl + "\">這裏</a> 重設密碼");
                // return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // 如果執行到這裡，發生某項失敗，則重新顯示表單
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // 不顯示使用者不存在
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // 要求重新導向至外部登入提供者
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // 產生並傳送 Token
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // 若使用者已經有登入資料，請使用此外部登入提供者登入使用者
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // 若使用者沒有帳戶，請提示使用者建立帳戶
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // 從外部登入提供者處取得使用者資訊
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Login", "Account");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Helper
        // 新增外部登入時用來當做 XSRF 保護
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            //return RedirectToAction("Login");
            return RedirectToAction("Index", "Home");
            //return View("~/Fhir_Demo/Home/Index.cshtml");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }

    //public class AccountController : Controller
    //{
    //    private FHIRResourceModel db = new FHIRResourceModel();
    //    //smart 網址參數設定
    //    private static string _clientId = "fhir_demo_id";
    //    private static string _authCode = "";
    //    private static string _clientState = "";
    //    private static string _redirectUrl = "https://localhost:44309/Account/HandleAuthCode";
    //    private static string _tokenUrl = "http://10.20.15.157:4013/v/r4/auth/token";
    //    private static string _fhirServerUrl = "http://10.20.15.157:4013/v/r4/fhir";
    //    private string token = "";

    //    //smart
    //    private static string tokenUrl =
    //        $"http://10.20.15.157:4013/v/r4/auth/authorize" +
    //        $"?response_type=code" +
    //        $"&client_id=whatever" +
    //        $"&redirect_uri={HttpUtility.UrlEncode(_redirectUrl)}" +
    //        $"&system scope={HttpUtility.UrlEncode("openid fhirUser profile launch/patient patient/*.read")}" +
    //        $"&state=local_state" +
    //        $"&aud={_fhirServerUrl}";

    //    [HttpGet]
    //    // GET: Account
    //    public ActionResult Login()
    //    {
    //        //註冊有登入信息時
    //        if (Session["NewUser"] != null)
    //        {
    //            var model = (User)Session["NewUser"];

    //            Session["NewUser"] = null;
    //            return View(model);
    //        }
    //        return View();
    //    }

    //    [HttpPost]
    //    [AllowAnonymous]
    //    [ValidateAntiForgeryToken]
    //    public ActionResult Login(User user)
    //    {
    //        if (!ModelState.IsValid)
    //        {
    //            return View(user);
    //        }

    //        //判斷帳密使否正確
    //        var existingUser = db.Users.Any(u => u.UserName == user.UserName && u.Password == user.Password);
    //        if (existingUser == true)
    //        {
    //            //儲存帳密
    //            Session["UserName"] = user.UserName;
    //            Session["Password"] = user.Password;
    //            TempData["message"] = "登入成功";

    //            //string returnUrl = Url.Action("LoginCallback", "User", null, Request.Scheme);
    //            //tokenUrl += "?returnUrl=" + returnUrl;

    //            //原本使用
    //            //string redirectUrl = Url.Action("HandleAuthCode", "Account", null, Request.Url.Scheme);
    //            //return Redirect(tokenUrl);

    //            return RedirectToAction("index", "Home", new { isLoggedIn = true});
                
    //        }
    //        else
    //        {
    //            TempData["validation"] = "帳號或密碼有誤，查無此帳號";
    //            return View(user);
    //        }
    //    }

    //    public ActionResult GetToken()
    //    {
    //        //string redirectUrl = Url.Action("HandleAuthCode", "Account", null, Request.Url.Scheme);
    //        //return Redirect(tokenUrl);

    //        //避免在Https網頁中導向Http網頁時出錯
    //        var client = new HttpClient();
    //        var response = client.GetAsync(tokenUrl).Result;

    //        if (response.IsSuccessStatusCode)
    //        {
    //            string content = response.Content.ReadAsStringAsync().Result;
    //            //return Content(content, "text/html");
    //            return Content(content, "text/plain");
    //            //return Json(new { Token= token});
    //        }
    //        else
    //        {
    //            //回傳失敗
    //            return Content("Error occurred while fetching content from the insecure page.");
    //        }
    //    }

    //    //拿取Acess_Token
    //    public async Task<string> SetAuthCode(string code, string state)
    //    {
    //        _authCode = code;
    //        _clientState = state;

    //        if (!string.IsNullOrEmpty(code) && !string.IsNullOrEmpty(state))
    //        {
    //            // 組裝請求所需的參數
    //            Dictionary<string, string> requestValues = new Dictionary<string, string>()
    //            {
    //                { "grant_type", "authorization_code" },
    //                { "code", code },
    //                { "redirect_uri", _redirectUrl },
    //                { "client_id", _clientId }
    //            };

    //            using (HttpClient client = new HttpClient())
    //            {
    //                try
    //                {
    //                    HttpRequestMessage request = new HttpRequestMessage()
    //                    {
    //                        Method = HttpMethod.Post,
    //                        RequestUri = new Uri(_tokenUrl),
    //                        Content = new FormUrlEncodedContent(requestValues)
    //                    };

    //                    //回傳抓到的網址POST碼
    //                    HttpResponseMessage response = await client.SendAsync(request);

    //                    if (response.IsSuccessStatusCode)
    //                    {
    //                        string json = await response.Content.ReadAsStringAsync();
    //                        SmartResponse smartResponse = JsonSerializer.Deserialize<SmartResponse>(json);
    //                        token = smartResponse.AccessToken;
    //                    }
    //                    //await Task.Run(() => DoSomethingWithToken(smartResponse));
    //                }
    //                catch(Exception ex)
    //                {
    //                    Console.WriteLine("An error occurred: " + ex.Message);
    //                }
    //            }
    //        }
    //        else
    //        {
    //            token = "";
    //        }

    //        return token;
    //    }

    //    public async Task<ActionResult> HandleAuthCode()
    //    {
    //        string code = Request["code"];
    //        string state = Request["state"];

    //        token =  await SetAuthCode(code, state);

    //        // 儲存 token 於 Session
    //        Session["Token"] = token;
    //        Session["FhirServerUrl"] = _fhirServerUrl;

    //        //return RedirectToAction("Index", "Home");
    //        return Content(token, "text/plain");
    //    }

    //    //拿到Token之後的處理
    //    //public static async void DoSomethingWithToken(SmartResponse smartResponse)
    //    //{
    //    //    if (smartResponse == null)
    //    //    {
    //    //        throw new ArgumentNullException(nameof(smartResponse));
    //    //    }
    //    //    if (string.IsNullOrEmpty(smartResponse.AccessToken))
    //    //    {
    //    //        throw new ArgumentNullException($"SMART Access Token is required!");
    //    //    }
    //    //    HttpClient httpClient = new HttpClient();
    //    //    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, tokenUrl);

    //    //    // 添加身分驗證標頭
    //    //    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", smartResponse.AccessToken);

    //    //    // 發送請求並獲取回應
    //    //    HttpResponseMessage response = await httpClient.SendAsync(request);

    //    //    //Hl7.Fhir.Rest.FhirClient fhirClient = new Hl7.Fhir.Rest.FhirClient(_fhirServerUrl);
    //    //    //fhirClient.HttpClientEventHandler.OnBeforeRequest += (sender, e) =>
    //    //    //{
    //    //    //    e.RawRequest.Headers.Add("Authorization", $"Bearer {smartResponse.AccessToken}");
    //    //    //};


    //    //    //Hl7.Fhir.Model.Patient patient = requestz.Read<Hl7.Fhir.Model.Patient>($"Patient/{smartResponse.PatientId}");

    //    //    //System.Console.WriteLine($"Read back patient: {patient.Name[0].ToString()}");
    //    //}

    //    [AllowAnonymous]
    //    public ActionResult LogOff()
    //    {
    //        Session["UserName"] = string.Empty;
    //        Session["Password"] = string.Empty;

    //        if (Session["Token"] != null)
    //        {
    //            Session["Token"] = null;
    //            Session["FhirServerUrl"] = null;
    //        }

    //        // 刪除 Cookie
    //        HttpCookie cookie = HttpContext.Request.Cookies["FHIR_url"];
    //        if (cookie != null)
    //        {
    //            cookie.Expires = DateTime.Now.AddDays(-1);
    //            HttpContext.Response.Cookies.Add(cookie);
    //        }
    //        HttpCookie tokenCookie = HttpContext.Request.Cookies["FHIR_Token"];
    //        if (tokenCookie != null)
    //        {
    //            tokenCookie.Expires = DateTime.Now.AddDays(-1);
    //            HttpContext.Response.Cookies.Add(tokenCookie);
    //        }

    //        return RedirectToAction("Index", "Home");
    //    }

    //    [HttpGet]
    //    [AllowAnonymous]
    //    public ActionResult Register()
    //    {
    //        return View();
    //    }

    //    [HttpPost]
    //    [AllowAnonymous]
    //    [ValidateAntiForgeryToken]
    //    public ActionResult Register(User user)
    //    {
    //        if (ModelState.IsValid)
    //        {
    //            //判斷帳密是否存在
    //            bool userexists = db.Users.Any(u => u.UserName == user.UserName && u.Password == user.Password);
    //            if (userexists)
    //            {
    //                TempData["exists"]  = "該帳號和密碼已被使用。";
    //            }
    //            else
    //            {
    //                var NewUser = new User
    //                {
    //                    UserName = user.UserName,
    //                    Password = user.Password,
    //                    RememberMe = true,
    //                    AuthorizeUrl = user.AuthorizeUrl,
    //                    Role = "User"
    //                };
    //                db.Users.Add(NewUser);
    //                db.SaveChanges();

    //                //確定存入資訊讓登入自動顯示
    //                Session["NewUser"] = NewUser;
    //                return RedirectToAction("Login");
    //            }
    //        }
    //        return View(user);
    //    }
    //}
}