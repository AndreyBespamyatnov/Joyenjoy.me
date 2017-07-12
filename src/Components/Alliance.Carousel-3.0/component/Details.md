
### Alliance Carousel ###

**Simply Add Carousel View as Subview of the Main View of View Controller**

Create the object of carousel View, add the appropriate data source and delegate and mention the carousel type. Add the carousel as subview of appropriate view (In this case Main View of the View Controller).

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