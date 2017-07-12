using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PhotoBooth.Models;
using Xamarin.Forms;
using Acr.UserDialogs;

namespace PhotoBooth.Mobile
{
    public class App : Application
    {
        public static List<PhotoBoothEntity> PhotoBoothEntities { get; set; }
        public static List<DashSquare> DashSquares { get; set; }

        static NavigationPage _navPage;
        public static string ShareLink { get; set; }

        public static Action SuccessfulLoginAction
        {
            get
            {
                return () =>
                {
                    _navPage.Navigation.PopModalAsync(true);
                };
            }
        }

        public App ()
		{
            _navPage = new NavigationPage(new MainPage());
            MainPage = _navPage;
        }

		protected override void OnStart ()
		{
			// Handle when your app starts
		}

		protected override void OnSleep ()
		{
			// Handle when your app sleeps
		}

		protected override void OnResume ()
		{
			// Handle when your app resumes
		}
    }
}
