using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Foundation;
using Newtonsoft.Json;
using PhotoBooth.Models;
using Xamarin.Forms;

namespace PhotoBooth.Mobile
{
    public class ExitAppException : Exception
    {
        public ExitAppException() { }
        public ExitAppException(string message) : base(message) { }
        public ExitAppException(string message, Exception inner) :
              base(message, inner)
        { }
    }

    public partial class MainPage : ContentPage
    {
        const int colIndexMax = 2;
        const int rowIndexMax = 1;
        List<Grid> dashboards;
        int pageIndex = 0;
        int currentPageIndex;
        private Timer t;

        private bool isBysi = false;
        private bool isFirstLoad = false;

        public MainPage()
        {
            InitializeComponent();

            NavigationPage.SetHasNavigationBar(this, false);

            NSTimer.CreateRepeatingScheduledTimer(10, async obj =>
            {
                if (!isBysi && !isFirstLoad)
                {
                    await LoadViewData();
                }
            });
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();

            await LoadViewData();
        }

        private async Task LoadViewData()
        {
            isBysi = true;

            App.PhotoBoothEntities = await GetPhotoBoothEntitys();

            if (Settings.PhotoBoothId == Guid.Empty)
            {
                await Navigation.PushModalAsync(new SelectPhotoBoothPage(), true);
                return;
            }

            var photoEvents = await GetPhotoEvent(Settings.PhotoBoothId);
            if (photoEvents == null)
            {
                await DisplayAlert("Нет события", "Сейчас не происходит ни одно событие.", "OK");

                if (Device.OS == TargetPlatform.iOS)
                {
                    throw new ExitAppException("known crash to exit app");
                }

                return;
            }

            var itemsCount = photoEvents.Photos.Count;
            if (App.DashSquares != null && itemsCount == App.DashSquares.Count)
            {
                return;
            }

            dashboards = new List<Grid>();
            var dashboard = new Grid()
            {
                ColumnSpacing = 0,
                RowSpacing = 0,
                VerticalOptions = LayoutOptions.FillAndExpand,
            };

            Enumerable.Range(0, rowIndexMax + 1).ToList().ForEach(x =>
                dashboard.RowDefinitions.Add(
                    new RowDefinition
                    {
                        Height = new GridLength(1, GridUnitType.Star)
                    }
                    ));

            Enumerable.Range(0, colIndexMax + 1).ToList().ForEach(x =>
                dashboard.ColumnDefinitions.Add(new ColumnDefinition
                {
                    Width = new GridLength(1, GridUnitType.Star)
                }));

            var colIndex = 0;
            var rowIndex = 0;

            App.DashSquares = new List<DashSquare>();
            foreach (var p in photoEvents.Photos.OrderByDescending(p=>p.Created))
            {
                if (p.IsDeleted)
                {
                    continue;
                }

                var item = new DashSquare
                {
                    BigImage = "http://joyenjoy.me/EventImages/" + p.PhotoEventId + "/" + p.ImageName,
                    PreviewImage = "http://joyenjoy.me/EventImages/" + p.PhotoEventId + "/" + p.PreviewImageName,
                    Id = p.Id,
                    Column = colIndex,
                    Row = rowIndex,
                    Page = pageIndex,
                    PhotoBoothId = photoEvents.PhotoBoothEntityId,
                    BlobPath = p.BlobPathToImage
                };

                var widget = new DashWidgetView(item);
                widget.Tapped += (sender, e) =>
                {
                    var page = new ActionPage();
                    Navigation.PushAsync(page, true);

                    MessagingCenter.Send<ActionPage, DashSquare>(page, "Hi", e.ImageObject);
                };

                dashboard.Children.Add(widget, colIndex, rowIndex);
                App.DashSquares.Add(item);

                colIndex++;

                if (colIndex > colIndexMax)
                {
                    colIndex = 0;
                    rowIndex++;
                }

                if (rowIndex > rowIndexMax)
                {
                    rowIndex = 0;
                    pageIndex ++;

                    dashboards.Add(dashboard);
                    dashboard = new Grid()
                    {
                        ColumnSpacing = 0,
                        RowSpacing = 0,
                        VerticalOptions = LayoutOptions.FillAndExpand,
                    };
                }
            }

            if (itemsCount%((colIndexMax + 1)*(rowIndexMax + 1)) != 0)
            {
                dashboards.Add(dashboard);
            }

            currentPageIndex = dashboards.Count - 1;
            if (currentPageIndex < 0)
            {
                currentPageIndex = 0;
            }

            var grid = dashboards.ElementAtOrDefault(currentPageIndex);
            if (grid != null)
            {
                ImageStackLayout.Children.Clear();
                ImageStackLayout.Children.Add(grid);
            }

            isFirstLoad = true;
            isBysi = false;
        }

        public static async Task<List<PhotoBoothEntity>> GetPhotoBoothEntitys()
        {
            var client = new HttpClient {BaseAddress = new Uri("http://joyenjoy.me/Api/")};
            var response = await client.GetAsync("PhotoBoothEntities/");
            var earthquakesJson = await response.Content.ReadAsStringAsync();
            var rootobject = JsonConvert.DeserializeObject<List<PhotoBoothEntity>>(earthquakesJson);

            return rootobject;
        }

        public static async Task<PhotoEvent> GetPhotoEvent(Guid photoBoothEntityId)
        {
            var client = new HttpClient {BaseAddress = new Uri("http://joyenjoy.me/Api/")};
            var response = await client.GetAsync("PhotoEvents/" + photoBoothEntityId);
            var earthquakesJson = await response.Content.ReadAsStringAsync();
            var rootobject = JsonConvert.DeserializeObject<PhotoEvent>(earthquakesJson);

            return rootobject;
        }

        private void OnRightButtonClicked(object sender, EventArgs e)
        {
            if (dashboards == null || !dashboards.Any())
            {
                return;
            }

            var nextPage = currentPageIndex + 1;
            var nextGrid = dashboards.ElementAtOrDefault(nextPage);
            if (nextPage > pageIndex || nextGrid == null)
            {
                return;
            }

            currentPageIndex = nextPage;
            ImageStackLayout.Children.Clear();
            ImageStackLayout.Children.Add(nextGrid);
        }

        private void OnLeftButtonClicked(object sender, EventArgs e)
        {
            if (dashboards == null || !dashboards.Any())
            {
                return;
            }

            var prevPage = currentPageIndex - 1;
            var prevGrid = dashboards.ElementAtOrDefault(prevPage);
            if (prevPage > pageIndex || prevGrid == null)
            {
                return;
            }

            currentPageIndex = prevPage;
            ImageStackLayout.Children.Clear();
            ImageStackLayout.Children.Add(prevGrid);
        }
    }
}
