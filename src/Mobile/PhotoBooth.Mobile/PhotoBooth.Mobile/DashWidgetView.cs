using System;
using FFImageLoading;
using Xamarin.Forms;

namespace PhotoBooth.Mobile
{
    public class WidgetTappedEventArgs : EventArgs
    {
        public DashSquare ImageObject { get; }
        public WidgetTappedEventArgs(DashSquare imageObject)
        {
            ImageObject = imageObject;
        }
    }

    public class DashSquare
    {
        public Guid Id { get; set; }
        public string PreviewImage { get; set; }
        public string BigImage { get; set; }

        public int Column { get; set; }

        public int Row { get; set; }
        public int Page { get; set; }


        public string BlobPath { get; set; }
        public Guid PhotoBoothId { get; set; }
    }

    public class DashWidgetView : ContentView
    {
        public event EventHandler<WidgetTappedEventArgs> Tapped;

        public DashWidgetView(DashSquare square)
        {
            RelativeLayout layout = new RelativeLayout();

            var tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.Tapped += (s, e) => {
                EventHandler<WidgetTappedEventArgs> handler = Tapped;
                handler?.Invoke(this, new WidgetTappedEventArgs(square));
            };

            layout.GestureRecognizers.Add(tapGestureRecognizer);

            var backgroundImage = new CachedImage()
            {
                Source = new UriImageSource
                {
                    CachingEnabled = true,
                    CacheValidity = new TimeSpan(5, 0, 0, 0),
                    Uri = new Uri(square.PreviewImage)
                },
                Aspect = Aspect.AspectFill,
                InputTransparent = false,

                CacheDuration = new TimeSpan(5, 0, 0, 0),
                LoadingPlaceholder = "placeholder.jpg"
            };

            layout.Children.Add(backgroundImage,
                Constraint.Constant(0),
                Constraint.Constant(0),
                Constraint.RelativeToParent((parent) => parent.Width),
                Constraint.RelativeToParent((parent) => parent.Height));
            
            this.Padding = new Thickness(10,10);
            Content = layout;
        }
    }
}
