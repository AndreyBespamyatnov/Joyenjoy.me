using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Acr.UserDialogs;
using Newtonsoft.Json;
using PhotoBooth.Models;
using UIKit;
using Xamarin.Auth;
using Xamarin.Forms;

namespace PhotoBooth.Mobile
{
    public partial class ActionPage : ContentPage
	{
        public DashSquare CurrentItem { get; set; }

		public ActionPage ()
		{
			InitializeComponent ();
            NavigationPage.SetHasNavigationBar(this, false);

            var returnComman = new Command(() =>
            {
                Navigation.PopAsync(true);
            });

            ImageStackLayout.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = returnComman
            });

            RelativeLayout layout = new RelativeLayout();

            var backgroundImage = new CachedImage()
            {
                Aspect = Aspect.AspectFit,
                InputTransparent = false,
                VerticalOptions = LayoutOptions.FillAndExpand,
                HorizontalOptions = LayoutOptions.FillAndExpand,

                CacheDuration = new TimeSpan(5, 0, 0, 0),
                LoadingPlaceholder = "placeholder.jpg"
            };

            layout.Children.Add(backgroundImage,
                Constraint.Constant(0),
                Constraint.Constant(0),
                Constraint.RelativeToParent((parent) => parent.Width),
                Constraint.RelativeToParent((parent) => parent.Height));

            ImageStackLayout.Children.Add(layout);

            MessagingCenter.Subscribe<ActionPage, DashSquare>(this, "Hi", (page, dashSquare) =>
            {
                CurrentItem = dashSquare;

                backgroundImage.Source = new UriImageSource
                {
                    CachingEnabled = true,
                    CacheValidity = new TimeSpan(5, 0, 0, 0),
                    Uri = new Uri(CurrentItem.BigImage)
                };

                var indicator = new ActivityIndicator
                {
                    Color = Color.FromHex("#009d48"),
                    BackgroundColor = Color.Transparent,
                    VerticalOptions = LayoutOptions.CenterAndExpand,
                    HorizontalOptions = LayoutOptions.CenterAndExpand,
                };
                layout.Children.Add(indicator,
                    Constraint.Constant(0),
                    Constraint.Constant(0),
                    Constraint.RelativeToParent((parent) => parent.Width),
                    Constraint.RelativeToParent((parent) => parent.Height));

                indicator.SetBinding(ActivityIndicator.IsRunningProperty, "IsLoading");
                indicator.BindingContext = backgroundImage;
            });
        }

        private async void OnPrintButtonClicked(object sender, EventArgs e)
        {
            try
            {
                UserDialogs.Instance.ShowLoading("Загрузка", MaskType.Black);
                PrintQueue print = new PrintQueue
                {
                    PhotoBoothEntityId = CurrentItem.PhotoBoothId,
                    BlobPathToImage = CurrentItem.BlobPath
                };

                var client = new HttpClient { BaseAddress = new Uri("http://joyenjoy.me/Api/") };
                var response = await client.PostAsync("PrintQueues/", new StringContent(JsonConvert.SerializeObject(print), Encoding.UTF8, "application/json"));

                UserDialogs.Instance.HideLoading();
            }
            catch (Exception ex)
            {
                UserDialogs.Instance.HideLoading();
                UserDialogs.Instance.ShowError("Что то сломалось, вот беда. Попробуйте ещё раз попозже.");
            }
        }

        public static Task<string> InputBox(INavigation navigation)
        {
            // wait in this proc, until user did his input 
            var tcs = new TaskCompletionSource<string>();

            var lblTitle = new Label { Text = "Email", HorizontalOptions = LayoutOptions.Center, FontAttributes = FontAttributes.Bold };
            var lblMessage = new Label { Text = "Введите адрес почты для отправки фото:" };
            var txtInput = new Entry { Text = "",  };

            var btnOk = new Button
            {
                Text = "Ok",
                WidthRequest = 100,
                BackgroundColor = Color.FromRgb(0.8, 0.8, 0.8),
            };
            btnOk.Clicked += async (s, e) =>
            {
                // close page
                var result = txtInput.Text;
                await navigation.PopModalAsync();
                // pass result
                tcs.SetResult(result);
            };

            var btnCancel = new Button
            {
                Text = "Отмена",
                WidthRequest = 100,
                BackgroundColor = Color.FromRgb(0.8, 0.8, 0.8)
            };
            btnCancel.Clicked += async (s, e) =>
            {
                // close page
                await navigation.PopModalAsync();
                // pass empty result
                tcs.SetResult(null);
            };

            var slButtons = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                Children = { btnOk, btnCancel },
            };

            var layout = new StackLayout
            {
                Padding = new Thickness(0, 40, 0, 0),
                VerticalOptions = LayoutOptions.StartAndExpand,
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                Orientation = StackOrientation.Vertical,
                Children = { lblTitle, lblMessage, txtInput, slButtons },
            };

            // create and show page
            var page = new ContentPage();
            page.Content = layout;
            navigation.PushModalAsync(page);
            // open keyboard
            txtInput.Focus();

            // code is waiting her, until result is passed with tcs.SetResult() in btn-Clicked
            // then proc returns the result
            return tcs.Task;
        }

        private async void OnMailButtonClicked(object sender, EventArgs e)
        {
            
            try
            {
                Email email = new Email
                {
                    To = await InputBox(this.Navigation),
                    PhotoId = CurrentItem.Id
                };

                UserDialogs.Instance.ShowLoading("Загрузка", MaskType.Black);
                var client = new HttpClient {BaseAddress = new Uri("http://joyenjoy.me/Api/")};
                var response = await client.PostAsync("Email/", new StringContent(JsonConvert.SerializeObject(email), Encoding.UTF8, "application/json"));

                UserDialogs.Instance.HideLoading();
            }
            catch (Exception ex)
            {
                UserDialogs.Instance.HideLoading();
                UserDialogs.Instance.ShowError("Что то сломалось, вот беда. Попробуйте ещё раз попозже.");
            }
        }

        private void OnVKButtonClicked(object sender, EventArgs e)
        {
            App.ShareLink = CurrentItem.BigImage;
            Navigation.PushModalAsync(new VKLoginPage());
        }

        private void OnFBButtonClicked(object sender, EventArgs e)
        {
            App.ShareLink = CurrentItem.BigImage;
            Navigation.PushModalAsync(new FBLoginPage());
        }

        private void OnLeftButtonClicked(object sender, EventArgs e)
        {
            var currentIndex = App.DashSquares.FindIndex(d => d.Id == CurrentItem.Id);
            var square = App.DashSquares.ElementAtOrDefault(currentIndex - 1);

            if(square != null)
            {
                MessagingCenter.Send<ActionPage, DashSquare>(this, "Hi", square);
            }
        }

        private void OnRightButtonClicked(object sender, EventArgs e)
        {
            var currentIndex = App.DashSquares.FindIndex(d => d.Id == CurrentItem.Id);
            var square = App.DashSquares.ElementAtOrDefault(currentIndex + 1);

            if (square != null)
            {
                MessagingCenter.Send<ActionPage, DashSquare>(this, "Hi", square);
            }
        }
    }
}
