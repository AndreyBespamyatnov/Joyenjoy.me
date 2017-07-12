using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using Foundation;
using UIKit;
using Alliance.Carousel;

namespace Sample
{
    public class ControlsViewController : UIViewController, ICarouselViewDelegate
    {
        public CarouselView Carousel;
        public UILabel Label;
        public HashSet<UIView> ObjCache = new HashSet<UIView>();

        public ControlsViewController() : base()
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Setup Background image
            var imgView = new UIImageView(UIImage.FromBundle("background"))
            {
                ContentMode = UIViewContentMode.ScaleToFill,
                AutoresizingMask = UIViewAutoresizing.All,
                Frame = View.Bounds
            };
            View.AddSubview(imgView);
			
            // Setup CarouselView view
            Carousel = new CarouselView(View.Bounds)
            {
                DataSource = new ControlsDataSource(this),
                Delegate = new CarouselViewDelegate()
            };
            Carousel.CarouselType = CarouselType.CoverFlow2;
            Carousel.ConfigureView();
			
            View.AddSubview(Carousel);

            // Setup info label
            Label = new UILabel(new RectangleF(20, 362, 280, 21))
            {
                BackgroundColor = UIColor.Clear,
                Text = string.Empty,
                TextAlignment = UITextAlignment.Center
            };

            View.AddSubview(Label);
        }

        public class ControlsDataSource : CarouselViewDataSource
        {
            ControlsViewController vc;

            public ControlsDataSource(ControlsViewController vc)
            {
                this.vc = vc;
            }

            public override nint NumberOfItems(CarouselView carousel)
            {
                // generate 100 item views
                // normally we'd use a backing array/List
                // as shown in the basic iOS example
                // but for this example we haven't bothered
                return 100;
            }

            public override UIView ViewForItem(CarouselView carousel, nint index, UIView reusingView)
            {
                if (reusingView == null)
                {

                    var itemView = new UIView(new RectangleF(0, 0, 200, 200))
                    {
                        AutoresizingMask = UIViewAutoresizing.All
                    };

                    // Creating the background
                    var imgView = new UIImageView(new RectangleF(-20, -90, 240, 380))
                    {
                        Image = UIImage.FromBundle("page"),
                        ContentMode = UIViewContentMode.Center
                    };
                    itemView.AddSubview(imgView);

                    // We create and add some controls
                    // UIButton
                    var button = UIButton.FromType(UIButtonType.RoundedRect);
                    button.Frame = new RectangleF(20, 20, 160, 37);
                    button.SetTitle("Press me!", UIControlState.Normal);
                    button.TouchUpInside += (sender, e) =>
                    {
                        vc.Label.Text = string.Format("Button {0} tapped", vc.Carousel.IndexOfItemViewOrSubview(sender as UIView));
                    };

                    if (!vc.ObjCache.Contains(button))
                        vc.ObjCache.Add(button);

                    itemView.AddSubview(button);

                    // UISwitch
                    var switchbtn = new UISwitch(new RectangleF(62, 86, 79, 27));
                    switchbtn.ValueChanged += (sender, e) =>
                    {
                        vc.Label.Text = string.Format("Switch {0} toggled", vc.Carousel.IndexOfItemViewOrSubview(sender as UIView));
                    };

                    if (!vc.ObjCache.Contains(switchbtn))
                        vc.ObjCache.Add(switchbtn);

                    itemView.AddSubview(switchbtn);

                    // UISlider
                    var slider = new UISlider(new RectangleF(41, 146, 118, 23))
                    {
                        MinValue = 0,
                        MaxValue = 100,
                        Value = 50
                    };
                    slider.ValueChanged += (sender, e) =>
                    {
                        vc.Label.Text = string.Format("Slider {0} changed", vc.Carousel.IndexOfItemViewOrSubview(sender as UIView));
                    };

                    if (!vc.ObjCache.Contains(slider))
                        vc.ObjCache.Add(slider);

                    itemView.AddSubview(slider);

                    reusingView = itemView;
                }
                return reusingView;
            }
        }
    }
}

