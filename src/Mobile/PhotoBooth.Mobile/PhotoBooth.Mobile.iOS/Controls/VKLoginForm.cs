using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using CoreGraphics;
using PhotoBooth.Mobile;
using PhotoBooth.Mobile.iOS.Controls;
using UIKit;
using Xamarin.Auth;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(VKLoginPage), typeof(VKLoginFormRenderer))]
namespace PhotoBooth.Mobile.iOS.Controls
{
    public class VKLoginFormRenderer : PageRenderer
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
                    clientId: "5054337",
                    scope: "wall",
                    authorizeUrl: new Uri("https://oauth.vk.com/authorize"),
                    redirectUrl: new Uri("https://oauth.vk.com/blank.html"));
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
            WebRequest webRequest = WebRequest.Create("https://api.vk.com/method/stats.trackVisitor?access_token=" + token);
            await webRequest.GetResponseAsync();

            webRequest = WebRequest.Create("https://api.vk.com/method/wall.post?message=joyenjoy.me&attachments=" + App.ShareLink + "&access_token=" + token);
            await webRequest.GetResponseAsync();

            //webRequest = WebRequest.Create("http://api.vk.com/oauth/logout");
            //await webRequest.GetResponseAsync();

            App.ShareLink = string.Empty;
        }
    }
}
