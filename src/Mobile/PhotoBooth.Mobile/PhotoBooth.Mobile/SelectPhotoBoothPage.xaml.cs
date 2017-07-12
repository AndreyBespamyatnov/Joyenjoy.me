using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PhotoBooth.Models;
using Xamarin.Forms;

namespace PhotoBooth.Mobile
{
	public partial class SelectPhotoBoothPage : ContentPage
	{
		public SelectPhotoBoothPage ()
		{
			InitializeComponent ();
        }

	    protected async override void OnAppearing()
	    {
	        base.OnAppearing();

            if (App.PhotoBoothEntities == null || !App.PhotoBoothEntities.Any())
            {
                App.PhotoBoothEntities = await MainPage.GetPhotoBoothEntitys();
            }

            PhotoBoothList.ItemsSource = App.PhotoBoothEntities;

            PhotoBoothList.ItemTemplate = new DataTemplate(typeof(TextCell));
            PhotoBoothList.ItemTemplate.SetBinding(TextCell.TextProperty, "Name");

            PhotoBoothList.ItemSelected += async (sender, e) =>
            {
                var boothEntity = (PhotoBoothEntity)e.SelectedItem;

                if (boothEntity == null)
                {
                    return;
                }

                await DisplayAlert("Будка установлена!", boothEntity.Name + " (" + boothEntity.Id + ").", "OK");
                Settings.PhotoBoothId = boothEntity.Id;
                await Navigation.PopModalAsync(true);
            };

	        PhotoBoothList.Refreshing += async (sender, args) =>
	        {
                App.PhotoBoothEntities = await MainPage.GetPhotoBoothEntitys();
            };
	    }
	}
}
