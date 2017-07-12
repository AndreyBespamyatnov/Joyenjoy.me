using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using CoreGraphics;
using FFImageLoading;
using FFImageLoading.Work;
using Foundation;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

namespace PhotoBooth.Mobile.iOS.Controls
{
    [Preserve(AllMembers = true)]
    public class CachedImageRenderer : ViewRenderer<CachedImage, UIImageView>
    {
        /// <summary>
        ///   Used for registration with dependency service
        /// </summary>
        public static void Init()
        {
        }

        private bool _isDisposed;

        protected override void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }

            UIImage image;
            if (disposing && base.Control != null && (image = base.Control.Image) != null)
            {
                image.Dispose();
            }

            _isDisposed = true;
            base.Dispose(disposing);
        }

        protected override void OnElementChanged(ElementChangedEventArgs<CachedImage> e)
        {
            if (Control == null)
            {
                SetNativeControl(new UIImageView(CGRect.Empty)
                {
                    ContentMode = UIViewContentMode.ScaleAspectFit,
                    ClipsToBounds = true
                });
            }
            if (e.NewElement != null)
            {
                SetAspect();
                SetImage(e.OldElement);
                SetOpacity();
            }
            base.OnElementChanged(e);
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == Image.SourceProperty.PropertyName)
            {
                SetImage(null);
            }
            if (e.PropertyName == Image.IsOpaqueProperty.PropertyName)
            {
                SetOpacity();
            }
            if (e.PropertyName == Image.AspectProperty.PropertyName)
            {
                SetAspect();
            }
        }

        private void SetAspect()
        {
            Control.ContentMode = Element.Aspect.ToUIViewContentMode();
        }

        private void SetOpacity()
        {
            Control.Opaque = Element.IsOpaque;
        }

        private void SetImage(CachedImage oldElement = null)
        {
            Xamarin.Forms.ImageSource source = base.Element.Source;
            if (oldElement != null)
            {
                Xamarin.Forms.ImageSource source2 = oldElement.Source;
                if (object.Equals(source2, source))
                {
                    return;
                }
                if (source2 is FileImageSource && source is FileImageSource && ((FileImageSource)source2).File == ((FileImageSource)source).File)
                {
                    return;
                }
            }

            ((IElementController)Element).SetValueFromRenderer(CachedImage.IsLoadingPropertyKey, true);

            TaskParameter imageLoader = null;

            if (source == null)
            {
                Control.Image = null;
                ImageLoadingFinished(Element);
            }
            else if (source is UriImageSource)
            {
                var urlSource = (UriImageSource)source;

                imageLoader = string.IsNullOrWhiteSpace(Element.LoadingPlaceholder) ? 
                    ImageService.Instance.LoadUrl(urlSource.Uri.ToString(), Element.CacheDuration) : 
                    ImageService.Instance.LoadUrl(urlSource.Uri.ToString(), Element.CacheDuration).LoadingPlaceholder(Element.LoadingPlaceholder);
            }
            else if (source is FileImageSource)
            {
                var fileSource = (FileImageSource)source;
                Control.Image = UIImage.FromBundle(fileSource.File);
                ImageLoadingFinished(Element);
            }
            else
            {
                throw new NotImplementedException("ImageSource type not supported");
            }

            if (imageLoader != null)
            {
                if ((int)Element.DownsampleHeight != 0 || (int)Element.DownsampleWidth != 0)
                {
                    if (Element.DownsampleHeight > Element.DownsampleWidth)
                    {
                        imageLoader.DownSample(height: (int)Element.DownsampleWidth);
                    }
                    else
                    {
                        imageLoader.DownSample(width: (int)Element.DownsampleHeight);
                    }
                }

                if (Element.RetryCount > 0)
                {
                    imageLoader.Retry(Element.RetryCount, Element.RetryDelay);
                }

                imageLoader.TransparencyChannel(Element.TransparencyEnabled);

                imageLoader.Finish((work) => ImageLoadingFinished(Element));
                imageLoader.Into(Control);
            }
        }

        void ImageLoadingFinished(CachedImage element)
        {
            if (element != null && !_isDisposed)
            {
                ((IElementController)element).SetValueFromRenderer(CachedImage.IsLoadingPropertyKey, false);
                ((IVisualElementController)element).NativeSizeChanged();
            }
        }

    }
}
