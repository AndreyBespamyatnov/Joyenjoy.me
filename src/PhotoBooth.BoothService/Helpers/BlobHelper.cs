using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Configuration;
using System.Drawing.Text;
using System.Linq;
using Microsoft.WindowsAzure;
using NLog;
using PhotoBooth.DAL;
using PhotoBooth.Models;

namespace PhotoBooth.BoothService.Helpers
{
    public class BlobHelper
    {
        private static BlobHelper _instance;
        private static ContextHelper _contextHelper;
        private static FilesHelper _filesHelper;

        private BlobHelper()
        {
            _contextHelper = ContextHelper.Instance;
            _filesHelper = FilesHelper.Instance;
        }

        public static Logger Log = LogManager.GetCurrentClassLogger();
        public string UploadFileToContainer(string filePath, string containerName, Logger logger)
        {
            string blobUrl = string.Empty;
            string fileName = new FileInfo(filePath).Name;

            StorageCredentials storageCredentials = new StorageCredentials(ConfigurationManager.AppSettings["AzureStorageAccountName"], ConfigurationManager.AppSettings["AzureStorageAccountKey"]);
            CloudStorageAccount storageAccount = new CloudStorageAccount(storageCredentials, false);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);
            container.CreateIfNotExists();
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(fileName);
            blockBlob.Properties.ContentType = GetContentType(filePath);

            try
            {
                using (var fileStream = File.OpenRead(filePath))
                {
                    blockBlob.UploadFromStream(fileStream);
                    blobUrl = blockBlob.Uri.AbsoluteUri;
                }
            }
            catch (Exception exception)
            {
                Log.Error("Cant upload image {0} to blob container {1}", filePath, containerName);
                Log.Error(exception);
            }

            if (!string.IsNullOrEmpty(blobUrl))
            {
                logger.Info("File {0} uploaded", filePath);
            }
            else
            {
                logger.Error("File {0} not uploaded!", filePath);
            }

            return blobUrl;
        }

        private string GetContentType(string filePath)
        {
            string mimeType = "application/unknown";
            string ext = Path.GetExtension(filePath).ToLower();
            Microsoft.Win32.RegistryKey regKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext);
            if (regKey != null && regKey.GetValue("Content Type") != null)
                mimeType = regKey.GetValue("Content Type").ToString();
            return mimeType;
        }

        public static BlobHelper Instance
        {
            get { return _instance ?? (_instance = new BlobHelper()); }
        }

      

        public void DownloadPhotoFromBlob(string containerName, string photoName, string targetFilePath)
        {
            StorageCredentials storageCredentials = new StorageCredentials(ConfigurationManager.AppSettings["AzureStorageAccountName"], ConfigurationManager.AppSettings["AzureStorageAccountKey"]);
            CloudStorageAccount storageAccount = new CloudStorageAccount(storageCredentials, false);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(photoName);
            blockBlob.DownloadToFile(targetFilePath, FileMode.Create);
        }
    }
}