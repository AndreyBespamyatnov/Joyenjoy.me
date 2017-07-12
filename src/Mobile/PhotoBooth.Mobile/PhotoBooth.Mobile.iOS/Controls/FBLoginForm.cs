using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using PhotoBooth.Mobile;
using PhotoBooth.Mobile.iOS.Controls;
using Xamarin.Auth;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(FBLoginPage), typeof(FBLoginForm))]
namespace PhotoBooth.Mobile.iOS.Controls
{
    public class FBLoginForm : PageRenderer
    {
        bool IsShown;

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            // Fixed the issue that on iOS 8, the modal wouldn't be popped.
            // url : http://stackoverflow.com/questions/24105390/how-to-login-to-facebook-in-xamarin-forms
            if (!IsShown)
            {
                IsShown = true;

                var auth = new OAuth2Authenticator(
                    clientId: "1492415574405370",
                    scope: "publish_actions, user_posts",
                    authorizeUrl: new Uri("https://www.facebook.com/dialog/oauth"),
                    redirectUrl: new Uri("https://www.facebook.com/connect/login_success.html"));
                auth.AllowCancel = true;
                auth.Completed += async (s, ee) =>
                {
                    // We presented the UI, so it's up to us to dimiss it on iOS.
                    App.SuccessfulLoginAction.Invoke();

                    if (ee.IsAuthenticated)
                    {
                        var token = ee.Account.Properties["access_token"];

                        await Share(token);
                    }
                    else
                    {
                        // The user cancelled
                    }
                };

                auth.Error += (sender, args) =>
                {
                    App.SuccessfulLoginAction.Invoke();
                };

                PresentViewController(auth.GetUI(), true, null);
            }
        }

        private async static Task Share(string token)
        {
            WebRequest webRequest = WebRequest.Create("https://graph.facebook.com/v2.4/me/feed?message=joyenjoy.me&link=" + App.ShareLink + "&access_token=" + token);
            webRequest.Method = "POST";
            await webRequest.GetResponseAsync();

            webRequest = WebRequest.Create("http://m.facebook.com/logout.php?confirm=1");
            await webRequest.GetResponseAsync();

            App.ShareLink = string.Empty;
        }
    }
}
