using System;
using System.Linq;

using CoreGraphics;
using UIKit;

using Carousels;

namespace BasiciOSExample
{
	public partial class ExampleViewController : UIViewController
	{
		UIImageView background;
		iCarousel carousel;
		UIToolbar toolbar;

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			bool wrap = false;

			// create a nice background
			background = new UIImageView (View.Bounds);
			background.Image = UIImage.FromBundle ("background.png");
			background.ContentMode = UIViewContentMode.ScaleToFill;
			background.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
			View.AddSubview (background);

			// create the carousel
			carousel = new iCarousel ();
			carousel.Type = iCarouselType.CoverFlow2;
			carousel.DataSource = new CarouselDataSource ();
			carousel.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
			View.AddSubview (carousel);

			// customize the appearance of the carousel
			carousel.GetValue = (sender, option, value) => {
				// set a nice spacing between items
				if (option == iCarouselOption.Spacing) {
					return value * 1.1F;
				} else if (option == iCarouselOption.Wrap) {
					return wrap ? 0 : 1;
				}

				// use the defaults for everything else
				return value;
			};

			// handle item selections
			carousel.ItemSelected += (sender, args) => {
				using (var alert = new UIAlertView("Item Selected", string.Format("You selected item '{0}'.", args.Index), null, "OK"))
					alert.Show();
			};

			toolbar = new UIToolbar ();
			toolbar.SetItems (new [] { 
				new UIBarButtonItem ("Change Style", UIBarButtonItemStyle.Plain, (sender, args) => {
					// create the popup
					UIActionSheet sheet = new UIActionSheet ("Select Carousel Type");
					var names = Enum.GetNames (typeof(iCarouselType));
					foreach (var type in names.Where(n => n != "Custom"))
						sheet.AddButton (type);
					// change the type
					sheet.Dismissed += (_, e) => {
						if (e.ButtonIndex != -1) {
							// animate the change
							UIView.BeginAnimations (null);
							carousel.Type = (iCarouselType)Enum.Parse (typeof(iCarouselType), names [e.ButtonIndex]);
							UIView.CommitAnimations ();
						}
					};
					// show the popup
					sheet.ShowInView (View);
				}),
				new UIBarButtonItem (UIBarButtonSystemItem.FlexibleSpace),
				new UIBarButtonItem ("Toggle Wrap", UIBarButtonItemStyle.Plain, (sender, args) => {
					wrap = !wrap;
					carousel.ReloadData ();
				})
			}, false);
			View.AddSubview (toolbar);
			toolbar.TranslatesAutoresizingMaskIntoConstraints = false;
			View.AddConstraints (new [] {
				NSLayoutConstraint.Create (toolbar, NSLayoutAttribute.Top, NSLayoutRelation.Equal, View, NSLayoutAttribute.Top, 1, 0),
				NSLayoutConstraint.Create (toolbar, NSLayoutAttribute.Left, NSLayoutRelation.Equal, View, NSLayoutAttribute.Left, 1, 0),
				NSLayoutConstraint.Create (toolbar, NSLayoutAttribute.Right, NSLayoutRelation.Equal, View, NSLayoutAttribute.Right, 1, 0),
			});
		}

private class CarouselDataSource : iCarouselDataSource
{
	int[] items;

	public CarouselDataSource()
	{
		// create our amazing data source
		items = Enumerable.Range (0, 100).ToArray ();
	}

	public override nint GetNumberOfItems (iCarousel carousel)
	{
		// return the number of items in the data
		return items.Length;
	}

	public override UIView GetViewForItem (iCarousel carousel, nint index, UIView view)
	{
		UILabel label = null;
		UIImageView imageView = null;

		if (view == null)
		{
			// create new view if no view is available for recycling
			imageView = new UIImageView(new CGRect(0, 0, 200.0f, 200.0f));
			imageView.Image = UIImage.FromBundle("page.png");
			imageView.ContentMode = UIViewContentMode.Center;

			label = new UILabel(imageView.Bounds);
			label.BackgroundColor = UIColor.Clear;
			label.TextAlignment = UITextAlignment.Center;
			label.Font = label.Font.WithSize(50);
			label.Tag = 1;
			imageView.AddSubview(label);
		}
		else
		{
			// get a reference to the label in the recycled view
			imageView = (UIImageView)view;
			label = (UILabel)view.ViewWithTag(1);
		}

		// set the values of the view
		label.Text = items [index].ToString ();

		return imageView;
	}
}
	}
}

