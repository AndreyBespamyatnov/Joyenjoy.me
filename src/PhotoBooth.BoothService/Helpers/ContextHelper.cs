using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using NLog;
using PhotoBooth.DAL;
using PhotoBooth.Models;

namespace PhotoBooth.BoothService.Helpers
{
    public class ContextHelper
    {
        readonly Guid _boothGuid = Guid.Parse(ConfigurationManager.AppSettings["BoothId"]);

        private static ContextHelper _instance;
        private ContextHelper() { }

        public bool IsDatabaseConnectionExist()
        {
            using (var db = new PhotoBoothContext())
            {
                DbConnection conn = db.Database.Connection;
                try
                {
                    conn.Open();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        public static ContextHelper Instance
        {
            get { return _instance ?? (_instance = new ContextHelper()); }
        }

        public bool AddPhoto(Photo photo, Logger logger)
        {
            bool result = false;
            try
            {
                using (var db = new PhotoBoothContext())
                {
                    if (photo.Created == DateTime.MinValue)
                    {
                        photo.Created = DateTime.Now;
                    }

                    db.Photos.Add(photo);
                    db.SaveChanges();
                }
                result = true;
                //logger.Info("Record for file {0} created", photo.LocalPathToImage);
            }
            catch (Exception ex)
            {
                logger.Error("Record for file {0} NOT created!", photo.LocalPathToImage);
                logger.Error(ex);
                if (ex.InnerException != null)
                {
                    logger.Error(ex.InnerException);
                    if (ex.InnerException.InnerException != null)
                    {
                        logger.Error(ex.InnerException.InnerException);
                    }
                }
            }

            return result;
        }

        public bool IsPhotoRecordExist(string filePath, Logger logger)
        {
            string md5 = FilesHelper.Instance.CalculateMd5(filePath);
            Photo photo = null;
            try
            {
                using (var db = new PhotoBoothContext())
                {
                    photo = db.Photos.FirstOrDefault(p => p.Md5Hash == md5);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
            return photo != null;
        }

        public bool SetBoothAvailable()
        {
            bool result = false;
            try
            {
                if (IsDatabaseConnectionExist())
                {
                    using (var db = new PhotoBoothContext())
                    {
                        var currentPhotoBooth = db.PhotoBooths.FirstOrDefault(pb => pb.Id == _boothGuid);
                        if (currentPhotoBooth != null) currentPhotoBooth.LastAvailableDate = DateTime.Now;
                        db.SaveChanges();
                        result = true;
                        LogManager.GetLogger("BoothAvailabilityLogger").Info("Availability time updated");
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.GetLogger("BoothAvailabilityLogger").Error(ex);
            }
            return result;
        }

        public bool IsInternetConnectionExist()
        {
            var result = false;
            try
            {
                Ping myPing = new Ping();
                String host = "google.com";
                byte[] buffer = new byte[32];
                int timeout = 5000;
                PingOptions pingOptions = new PingOptions();
                PingReply reply = myPing.Send(host, timeout, buffer, pingOptions);
                if (reply.Status == IPStatus.Success)
                {
                    result = true;
                }
            }
            catch (Exception)
            {
                result = false;
            }

            return result;
        }

        public List<PrintQueue> GetCurrentBoothPrintPhotos()
        {
            List<PrintQueue> currentPhotoBoothPrintElements = null;
            try
            {
                if (IsDatabaseConnectionExist())
                {
                    using (var db = new PhotoBoothContext())
                    {
                        currentPhotoBoothPrintElements = db.PrintQueue.Where(p => p.PhotoBoothEntityId == _boothGuid).ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.GetLogger("BoothAvailabilityTask").Error(ex);
            }
            return currentPhotoBoothPrintElements;
        }

        public void RemovePrintQueueElement(PrintQueue photoBoothPrintElement)
        {
            if (IsDatabaseConnectionExist())
            {
                using (var db = new PhotoBoothContext())
                {
                    try
                    {
                        PrintQueue objectToDelete = db.PrintQueue.FirstOrDefault(p => p.BlobPathToImage == photoBoothPrintElement.BlobPathToImage);
                        db.PrintQueue.Remove(objectToDelete);
                        db.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        LogManager.GetLogger("PrintTask").Error(ex);
                    }
                }
            }
        }
    }
}