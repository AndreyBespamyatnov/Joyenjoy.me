using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using PhotoBooth.Models;

namespace PhotoBooth.BoothService.Helpers
{
    public class ImageHelper
    {
        private static ImageHelper _instance;
        private readonly SettingsHelper _settingsHelper;
        private readonly BlobHelper _blobHelper;
        private readonly FilesHelper _filesHelper;
        public static Logger Log = LogManager.GetCurrentClassLogger();
        readonly int _dslrFolderRefreshTimeIntervalInSeconds = int.Parse(ConfigurationManager.AppSettings["DSLRFolderRefreshTimeIntervalInSeconds"]);

        private ImageHelper()
        {
            _settingsHelper = SettingsHelper.Instance;
            _blobHelper = BlobHelper.Instance;
            _filesHelper = FilesHelper.Instance;
            //Settings.DslrPhotoDirectoryPath = _settingsHelper.SetDslrPhotoDirectoryPath();
            // Settings.SaveChanges();
        }

        public string GetImageThumbnail(string imagePath)
        {
            string thumbnailFilePath;
            
            using (FileStream fs = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
            {
                using (Image image = Image.FromStream(fs))
                {
                    FileInfo fileInfo = new FileInfo(imagePath);
                    double hRatio = (double)image.Height / 200;
                    //SizeF newSize = hRatio < 1 ? new SizeF(image.Width, image.Height) : new SizeF((float)(image.Width / hRatio), (float)(image.Height / hRatio));
                    SizeF newSize = new SizeF(150,150);

                    Image thumb = image.GetThumbnailImage((int)newSize.Width, (int)newSize.Height, () => false, IntPtr.Zero);
                    thumbnailFilePath = Path.Combine(fileInfo.DirectoryName, Path.GetFileNameWithoutExtension(imagePath) + "_preview" + fileInfo.Extension);
                    thumb.Save(thumbnailFilePath);
                }
            }
            return thumbnailFilePath;
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
    }
}