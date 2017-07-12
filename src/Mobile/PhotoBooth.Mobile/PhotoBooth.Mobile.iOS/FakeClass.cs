using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace System.Web
{
    public class HttpPostedFileBase
    {
        public virtual int ContentLength
        {
            get { return 0; }
        }


        public virtual Stream InputStream
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }
}

namespace System.Drawing.Imaging
{
    public sealed class ImageFormat
    {
        private static ImageFormat png = new ImageFormat(new Guid("{b96b3caf-0728-11d3-9d7b-0000f81ef32e}"));
        private Guid guid;

        public ImageFormat()
        {
        }

        public ImageFormat(Guid guid)
        {
            this.guid = guid;
        }

        public static ImageFormat Png
        {
            get
            {
                return ImageFormat.png;
            }
        }
    }

    public class Image : IDisposable
    {
        public static Image FromStream(Stream stream)
        {
            return new Image();
        }

        public Size Size
        {
            get
            {
                return new Size(0, 0);
            }
        }

        public ImageFormat RawFormat
        {
            get
            {
                return new ImageFormat();
            }
        }

        public void Dispose()
        {
            
        }
    }
}

