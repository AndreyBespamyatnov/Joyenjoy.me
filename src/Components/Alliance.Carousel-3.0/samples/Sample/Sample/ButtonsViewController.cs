using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using CoreGraphics;
using Foundation;
using UIKit;
using Alliance.Carousel;

namespace Sample
{
    public class ButtonsViewController : UIViewController
    {
        public List<int> items;
        public CarouselView carousel;
        public HashSet<UIView> objCache = new HashSet<UIView>();

        public ButtonsViewController() : base()
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Setup the item list we will display
            // your carousel should always be driven by an array/list of
            // data of some kind - don't store data in your item views
            // or the recycling mechanism will destroy your data once
            // your item views move off-screen
            items = Enumerable.Range(1, 100).ToList(); // Prettier than for (nint i = 0; i < 100; i++)
			
            // Setup Background image
            var imgView = new UIImageView(UIImage.FromBundle("background"))
            {
                ContentMode = UIViewContentMode.ScaleToFill,
                AutoresizingMask = UIViewAutoresizing.All,
                Frame = View.Bounds
            };
            View.AddSubview(imgView);
			
            // Setup CarouselView view
            carousel = new CarouselView(View.Bounds)
            {
                DataSource = new ButtonsDataSource(this),
                Delegate = new CarouselViewDelegate()
            };
            carousel.CarouselType = CarouselType.CoverFlow2;
            carousel.ConfigureView();
			
            View.AddSubview(carousel);
        }

        public class ButtonsDataSource : CarouselViewDataSource
        {
            ButtonsViewController vc;

            public ButtonsDataSource(ButtonsViewController vc)
            {
                this.vc = vc;
            }

            public override nint NumberOfItems(CarouselView carousel)
            {
                return (nint)vc.items.Count;
            }

            public override UIView ViewForItem(CarouselView carousel, nint index, UIView reusingView)
            {
                var button = reusingView as UIButton;
                if (button == null)
                {
                    //no button available to recycle, so create new one
                    var image = UIImage.FromBundle("page.png");
                    button = UIButton.FromType(UIButtonType.Custom);
                    button.Frame = new CGRect(0, 0, image.Size.Width, image.Size.Height);
                    button.SetTitleColor(UIColor.Black, UIControlState.Normal);
                    button.SetBackgroundImage(image, UIControlState.Normal);
                    button.TitleLabel.Font = button.TitleLabel.Font.WithSize(50);
                    button.TouchUpInside += (sender, e) =>
                    {
                        var idx = vc.carousel.IndexOfItemViewOrSubview(sender as UIView);
                        new UIAlertView("Hello", "You tapped button number " + idx, null, "Ok", null).Show();
                    };
                }

                // set button label
                button.SetTitle(index.ToString(), UIControlState.Normal);

                if (!vc.objCache.Contains(button))
                    vc.objCache.Add(button);

                return button;
            }
        }
    }
}

