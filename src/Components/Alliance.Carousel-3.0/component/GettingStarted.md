### Alliance Carousel ###
Alliance Carousel is Xamarin.iOS port of objective-c based iCarousel control library using C# (It doesnot use MonoTouch objective-c binding technique that exposes objective-c based objects as c# proxy objects). Most of the features of iCarousel control has been ported. 

**Alliance Carousel provides following features**

1. Diffrent types of UIViews (UIView, UIImageView, Custom UIView and others) can be used in carousel
2. Views used in carousel can be of static or dynamic nature (loading view or view components on the fly e.g. loading UIImageView dynamically from image url)
3. Both vertical and horizonal orientation are supported.
4. Both manual, programatic and auto scroll features are avialable.
5. Can accept 0 items in carousel
6. and many more........

**Different types of carousel available in Alliance Carousel**

1. Linear
2. Rotary
3. Cylinder
4. Wheel
5. CoverFlow
6. Time Machine

####How to use####

**Include the below assembly**

    using Alliance.Carousel;

**Code the  Carousel DataSource**

1. Inherit the CarouselViewDataSource class to create a suitable data source for your carousel. Inheriting is must as  CarouselViewDataSource is abstract class
2. Pass the view controller through constructor
3. Implement the abstract methods and if required override the virtual methods as per requirement. In this example NumberOfItems and ViewForItem methods are implemented.
4. DataSource is mandatory for CarouselView

**Code**

    	public class LinearDataSource : CarouselViewDataSource
        {
			LinearViewController vc;

            public LinearDataSource(LinearViewController vc)
            {
                this.vc = vc;
            }

            public override uint NumberOfItems(CarouselView carousel)
            {
                return (uint)vc.items.Count;
            }

            public override UIView ViewForItem(CarouselView carousel, uint index, UIView reusingView)
            {

                UILabel label;

                if (reusingView == null)
                {
                    var imgView = new UIImageView(new RectangleF(0, 0, 200, 200))
                    {
                        Image = UIImage.FromBundle("page"),
                        ContentMode = UIViewContentMode.Center
                    };

                    label = new UILabel(imgView.Bounds)
                    {
                        BackgroundColor = UIColor.Clear,
                        TextAlignment = UITextAlignment.Center,
                        Tag = 1
                    };
                    label.Font = label.Font.WithSize(50);
                    imgView.AddSubview(label);
                    reusingView = imgView;
                }
                else
                {
                    label = (UILabel)reusingView.ViewWithTag(1);
                }
				
                label.Text = vc.items[(int)index].ToString();

                return reusingView;
            }
        }

**Code the Carousel Delegate**

1. Inherit the CarouselViewDelegate class to create a suitable delegate for your carousel (You can use the default CarouselViewDelegate class itself instead of inherting it and skip the step 2, 3 and 4)
2. Pass the view controller through constructor
3. Override the required methods as per requirement. In this example ValueForOption and DidSelectItem methods are overriden.
4. Delegate is mandatory for carousel

**Code**

	public class LinearDelegate : CarouselViewDelegate
		{
            LinearViewController vc;

            public LinearDelegate(LinearViewController vc)
            {
                this.vc = vc;
            }

            public override float ValueForOption(CarouselView carousel, CarouselOption option, float aValue)
            {
                if (option == CarouselOption.Spacing)
                {
                    return aValue * 1.1f;
                }
                return aValue;
            }

            public override void DidSelectItem(CarouselView carousel, int index)
            {
                Console.WriteLine("Selected: " + ++index);
            }
        }

**Create the CarouselView and add it to view of  view controller**

1. Create the view controller and override the ViewDidLoad method
2. Initialize the items that will be used to build the carousel in View Controller
3. Instantiate the CarouselView (using the RectangleF to specify the size and position of the CarouselView) and assign DataSource and Delegate
4. Assign the CarouselType, in this example we are building linear craousel
5. Call the ConfigureView method of the CarouselView
6. Add the CarouselView to the view of the view controller.

**Code**

        public class LinearViewController : UIViewController
		{
    		public List<int> items;
			CarouselView carousel;

	        public LinearViewController() : base()
	        {
	        }
	
	        public override void ViewDidLoad()
	        {
	            base.ViewDidLoad();
	
	            items = Enumerable.Range(1, 100).ToList();
	
	            var imgView = new UIImageView(UIImage.FromBundle("background"))
	            {
	                ContentMode = UIViewContentMode.ScaleToFill,
	                AutoresizingMask = UIViewAutoresizing.All,
	                Frame = View.Bounds
	            };
	            View.AddSubview(imgView);
	
	            carousel = new CarouselView(View.Bounds);
	            carousel.DataSource = new LinearDataSource(this);
	            carousel.Delegate = new LinearDelegate(this);
	            carousel.CarouselType = CarouselType.Linear;
	            carousel.ConfigureView();
	            View.AddSubview(carousel);
			}
		}

###Some additional Information###

**How to specify the size and position of CarouselView**

We can instantiate the CarouselView with position and size using RectangleF

    new CarouselView(new RectangleF(10, 25, 300, 400));

We can also use frame property to specify the size and position of CarouselView

    carousel.Frame = new RectangleF(10, 25, 300, 400);

> Note: We must specify the size and position of CarouselView and call the the ConfigureView method before adding it to any view.

    carousel.ConfigureView();
	View.AddSubView(carousel);

**How to change the CarouselType**

We can change the CarouselType e.g.

    carousel.CarouselType = CarouselType.CoverFlow

**How to change the orientation**

We can change the default orientation (horizontal) e.g.

    carousel.Vertical = true;

**How to auto scroll the items in CarouselView**

We can auto scroll the carousel items e.g.

    carousel.Autoscroll = 1.0f;

**How to scroll the items programatically in CarouselView**

We can scroll items programatically using different scroll methods avialable in CarouselView e.g.

    carousel.ScrollByNumberOfItems(2,0.1f);

Or,

    carousel.ScrollToItem(5,0.1f);

Or,

    carousel.ScrollByOffset(1.0f,0.1f);

Or,

    carousel.ScrollToOffset(1.0f,0.1f);

> Note: When scrolling using offset, the offset value must be >= 1.0f so that there is a clear visible impact. When using offset, index or number of items to scroll, developer must take care about "Index Out of Array" 