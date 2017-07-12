using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using NLog;
using PhotoBooth.Models;

namespace PhotoBooth.Service.Helpers
{
    public class ImageHelper
    {
        private static ImageHelper _instance;
        public static Logger Log = LogManager.GetCurrentClassLogger();

        public string GetImageThumbnail(string imagePath)
        {
            string thumbnailFilePath;

            using (FileStream fs = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
            {
                using (Image image = Image.FromStream(fs))
                {
                    FileInfo fileInfo = new FileInfo(imagePath);
                    double hRatio = (double)image.Height / 300;
                    SizeF newSize = hRatio < 1 ? new SizeF(image.Width, image.Height) : new SizeF((float)(image.Width / hRatio), (float)(image.Height / hRatio));
                    //SizeF newSize = new SizeF(150,150);

                    Image thumb = image.GetThumbnailImage((int)newSize.Width, (int)newSize.Height, () => false, IntPtr.Zero);
                    thumbnailFilePath = Path.Combine(fileInfo.DirectoryName, Path.GetFileNameWithoutExtension(imagePath) + "_preview" + fileInfo.Extension);
                    thumb.Save(thumbnailFilePath);
                }
            }
            return thumbnailFilePath;
        }

        public bool AddBorderToImage(string imagePath)
        {
            bool result = false;
            Bitmap outputImage = null;
            try
            {
                LogManager.GetLogger("InstagramSearchLogger").Info($"Lets try to add border for: {imagePath}");

                using (Image targetImage = Image.FromFile(imagePath))
                {
                    string borderBase64 = Settings.BrandImage;
                    using (Image borderImage = Base64ToImage(borderBase64))
                    {
                        outputImage = new Bitmap(targetImage.Width, targetImage.Height + borderImage.Height, PixelFormat.Format32bppArgb);
                        using (Graphics graphics = Graphics.FromImage(outputImage))
                        {
                            graphics.DrawImage(targetImage, new Rectangle(new Point(), targetImage.Size), new Rectangle(new Point(), targetImage.Size), GraphicsUnit.Pixel);
                            graphics.DrawImage(borderImage, new Rectangle(new Point(0, targetImage.Height), borderImage.Size), new Rectangle(new Point(), borderImage.Size), GraphicsUnit.Pixel);
                        }
                    }
                }
                
                bool isFileLocked = FilesHelper.Instance.IsFileLocked(new FileInfo(imagePath));
                LogManager.GetLogger("InstagramSearchLogger").Info($"File locked: {isFileLocked}");

                LogManager.GetLogger("InstagramSearchLogger").Info($"Try to delete file: {imagePath}");
                Thread.Sleep(TimeSpan.FromSeconds(1));
                File.Delete(imagePath);

                LogManager.GetLogger("InstagramSearchLogger").Info($"Lets try to save: {imagePath}");
                outputImage.Save(imagePath, ImageFormat.Jpeg);
                LogManager.GetLogger("InstagramSearchLogger").Info("Saved!");
                outputImage.Dispose();

                using (Image resultImage = Image.FromFile(imagePath))
                {
                    if (resultImage.Width == 640 || resultImage.Height == 960)
                    {
                        result = true;
                    }
                }
            }
            catch (Exception exception)
            {
                Log.Error(exception);
                LogManager.GetLogger("InstagramSearchLogger").Error(exception);
            }

            return result;
        }

        public Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        public Image Base64ToImage(string base64String)
        {
            // Convert Base64 String to byte[]
            byte[] imageBytes = Convert.FromBase64String(base64String);
            // Convert by7te[] to Image
            using (var ms = new MemoryStream(imageBytes, 0, imageBytes.Length))
            {
                Image image = Image.FromStream(ms, true);
                return image;
            }
        }

        internal List<Photo> Except(List<Photo> localPhotos, List<Photo> blobPhotos)
        {//not for all options
            var blobMissingFiles = new List<Photo>();
            bool isLocalFileExistInBlob = false;

            foreach (var localFile in localPhotos)
            {
                foreach (var blobFile in blobPhotos)
                {
                    if (localFile.Md5Hash == blobFile.Md5Hash)
                    {
                        isLocalFileExistInBlob = true;
                    }
                }
                if (!isLocalFileExistInBlob)
                {
                    blobMissingFiles.Add(localFile);
                }
                isLocalFileExistInBlob = false;
            }

            return blobMissingFiles;
        }

        public static ImageHelper Instance
        {
            get { return _instance ?? (_instance = new ImageHelper()); }
        }

        public void AddBorderToImages(List<string> localFilesList)
        {
            foreach (var filePath in localFilesList)
            {
                try
                {
                    LogManager.GetLogger("InstagramSearchLogger").Info($"Lets try to add border for: {filePath}");
                    Bitmap instagramImage = new Bitmap(filePath);

                    string borderBase64 = Settings.BrandImage;
                    Image borderImage = Base64ToImage(borderBase64);

                    Bitmap outputImage = new Bitmap(instagramImage.Width, instagramImage.Height + borderImage.Height, PixelFormat.Format32bppArgb);
                    using (Graphics graphics = Graphics.FromImage(outputImage))
                    {
                        graphics.DrawImage(instagramImage, new Rectangle(new Point(), instagramImage.Size), new Rectangle(new Point(), instagramImage.Size), GraphicsUnit.Pixel);
                        graphics.DrawImage(borderImage, new Rectangle(new Point(0, instagramImage.Height), borderImage.Size), new Rectangle(new Point(), borderImage.Size), GraphicsUnit.Pixel);
                    }
                    instagramImage.Dispose();
                    borderImage.Dispose();

                    LogManager.GetLogger("InstagramSearchLogger").Info($"Try to delete file: {filePath}");
                    File.Delete(filePath);

                    LogManager.GetLogger("InstagramSearchLogger").Info($"Lets try to save: {filePath}");
                    outputImage.Save(filePath, ImageFormat.Jpeg);
                    LogManager.GetLogger("InstagramSearchLogger").Info("Saved!");
                    outputImage.Dispose();
                }
                catch (Exception ex)
                {
                    LogManager.GetLogger("InstagramSearchLogger").Error(ex);
                }
            }
        }
    }
}