using System;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security;
using Owin;
using FHIR_Demo.Models;
using Microsoft.Owin.Security.Google;
using Owin.Security.Providers.LinkedIn;
using Microsoft.Owin.Security.Twitter;

namespace FHIR_Demo
{
    public partial class Startup
    {
        // 如需設定驗證的詳細資訊，請瀏覽 https://go.microsoft.com/fwlink/?LinkId=301864
        public void ConfigureAuth(IAppBuilder app)
        {
            // 設定資料庫內容、使用者管理員和登入管理員，以針對每個要求使用單一執行個體
            app.CreatePerOwinContext(ApplicationDbContext.Create);
            app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
            app.CreatePerOwinContext<ApplicationSignInManager>(ApplicationSignInManager.Create);

            // 讓應用程式使用 Cookie 儲存已登入使用者的資訊
            // 並使用 Cookie 暫時儲存使用者利用協力廠商登入提供者登入的相關資訊；
            // 在 Cookie 中設定簽章
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Account/Login"),
                Provider = new CookieAuthenticationProvider
                {
                    // 讓應用程式在使用者登入時驗證安全性戳記。
                    // 這是您變更密碼或將外部登入新增至帳戶時所使用的安全性功能。  
                    OnValidateIdentity = SecurityStampValidator.OnValidateIdentity<ApplicationUserManager, ApplicationUser>(
                        validateInterval: TimeSpan.FromMinutes(30),
                        regenerateIdentity: (manager, user) => user.GenerateUserIdentityAsync(manager))
                }
            });            
            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            // 讓應用程式在雙因素驗證程序中驗證第二個因素時暫時儲存使用者資訊。
            app.UseTwoFactorSignInCookie(DefaultAuthenticationTypes.TwoFactorCookie, TimeSpan.FromMinutes(5));

            // 讓應用程式記住第二個登入驗證因素 (例如電話或電子郵件)。
            // 核取此選項之後，將會在用來登入的裝置上記住登入程序期間的第二個驗證步驟。
            // 這類似於登入時的 RememberMe 選項。
            app.UseTwoFactorRememberBrowserCookie(DefaultAuthenticationTypes.TwoFactorRememberBrowserCookie);

            // 註銷下列各行以啟用利用協力廠商登入提供者登入
            //app.UseMicrosoftAccountAuthentication(
            //    clientId: "",
            //    clientSecret: "");

            app.UseTwitterAuthentication(
               consumerKey: "jG11zRT00zOw1eDjZLPQ60x9t",
               consumerSecret: "dFSoiBEyLJXpbFgbilODVFviNAIqoDXmZEoeJTxHDEA6SnzETG");

            //app.UseFacebookAuthentication(
            //   appId: "",
            //   appSecret: "");

            app.UseLinkedInAuthentication(new LinkedInAuthenticationOptions()
            {
                ClientId = "77g5sc0mbjhjgg",
                ClientSecret = "OCkDAig63akb02nx",
            });

            app.UseGoogleAuthentication(new GoogleOAuth2AuthenticationOptions()
            {
                ClientId = "960251954520-116vhlko59qlf2h631aapv9isb5i41m6.apps.googleusercontent.com",
                ClientSecret = "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"
            });
        }
    }
}