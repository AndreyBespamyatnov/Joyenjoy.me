using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading;
using NLog;
using PhotoBooth.Models;

namespace PhotoBooth.Service.Helpers
{
    public class FilesHelper
    {
        private static FilesHelper _instance;
        public static string DslrPhotoDirPath = ConfigurationManager.AppSettings["dslrPhotosDirPath"];
        public static string WorkDirPath = ConfigurationManager.AppSettings["serviceWorkDirPath"];
        public static string dslrCompletedPhotosDirName = ConfigurationManager.AppSettings["dslrCompletedPhotosDirName"];

        private const string ScFileName = "ScProcessedFileNames.csv";
        private const string ThFileName = "ThProcessedFileNames.csv";
        private const string InstagramFileName = "InstagramsProcessedFileNames.csv";

        private FilesHelper() { }

        public string CalculateMd5(string flePath)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(flePath))
                {
                    return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLower();
                }
            }
        }

        public int GetImageHeight(string filePath)
        {
            Image image = Image.FromFile(filePath);
            return image.Height;
        }

        public int GetImageWidth(string filePath)
        {
            Image image = Image.FromFile(filePath);
            return image.Width;
        }

        public void WaitForFile(string filepath)
        {
            FileInfo fileInfo = new FileInfo(filepath);
            FileStream stream = null;
            bool FileReady = false;
            while (!FileReady)
            {
                try
                {
                    using (fileInfo.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                    {
                        FileReady = true;
                    }
                }
                catch (IOException)
                { }
                if (!FileReady) Thread.Sleep(1000);
            }
        }

        public bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException)
            {
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return false;
        }

        public static FilesHelper Instance
        {
            get { return _instance ?? (_instance = new FilesHelper()); }
        }

        public void ScAddProcessedFile(string fileName)
        {
            CreateFileIfNotExist(ScFileName);

            List<string> alreadyProcessedFiles = File.ReadAllLines(ScFileName).ToList();
            if (!alreadyProcessedFiles.Contains(fileName))
            {
                alreadyProcessedFiles.Add(fileName);
                File.WriteAllLines(ScFileName, alreadyProcessedFiles);
            }
        }

        public void InstagramAddProcessedFile(string fileName)
        {
            CreateFileIfNotExist(InstagramFileName);

            List<string> alreadyProcessedFiles = File.ReadAllLines(InstagramFileName).ToList();
            if (!alreadyProcessedFiles.Contains(fileName))
            {
                alreadyProcessedFiles.Add(fileName);
                File.WriteAllLines(InstagramFileName, alreadyProcessedFiles);
            }
        }

        public List<string> ScGetAlreadyProcessedFiles()
        {
            List<string> result = GetAlreadyProcessedFiles(ScFileName);
            return result;
        }

        public List<string> ThGetAlreadyProcessedFiles()
        {
            List<string> result = GetAlreadyProcessedFiles(ThFileName);
            return result;
        }

        public List<string> InstagramGetAlreadyProcessedFiles()
        {
            List<string> result = GetAlreadyProcessedFiles(InstagramFileName);
            return result;
        }

        public List<string> GetAlreadyProcessedFiles(string tempFileName)
        {
            CreateFileIfNotExist(tempFileName);
            WaitForFile(tempFileName);
            List<string> alreadyProcessedFiles = File.ReadAllLines(tempFileName).ToList();
            return alreadyProcessedFiles;
        }

        public void CreateFileIfNotExist(string tempFileName)
        {
            if (!File.Exists(tempFileName))
            {
                File.Create(tempFileName);
            }
        }

        public string GetEventIdFromPath(string file)
        {
            string pattern = ".{8}-.{4}-.{4}-.{4}-.{12}";
            string eventId = Regex.Match(file, pattern).Groups[0].Value;
            return eventId;
        }

        public List<string> GetWorkDirFiles()
        {
            if (!Directory.Exists(WorkDirPath))
            {
                Directory.CreateDirectory(WorkDirPath);
            }
            List<string> files =
                new DirectoryInfo(WorkDirPath).GetFiles("*.*", SearchOption.AllDirectories)
                    .Select(f => f.FullName)
                    .ToList();
            return files;
        }

        public void ThAddProcessedFile(string file)
        {
            CreateFileIfNotExist(ThFileName);

            List<string> alreadyProcessedFiles = File.ReadAllLines(ThFileName).ToList();
            if (!alreadyProcessedFiles.Contains(file))
            {
                alreadyProcessedFiles.Add(file);
                File.WriteAllLines(ThFileName, alreadyProcessedFiles);
            }
        }

        public void PrintFile(string localPath)
        {
            PrinterSettings newSettings = new PrinterSettings() { };
            PrintDocument printDoc = new PrintDocument
            {
                PrintController = new StandardPrintController(),
                PrinterSettings = newSettings,
                DocumentName = Path.GetFileName(localPath)
            };
            printDoc.PrinterSettings.PrinterName = newSettings.PrinterName;
            printDoc.DefaultPageSettings.Landscape = false;
            Logger PrintTaskLogger = LogManager.GetLogger("PrintTaskLogger");
            PrintTaskLogger.Info("Printer: {0}", printDoc.PrinterSettings.PrinterName);

            printDoc.DefaultPageSettings.PaperSize = new PaperSize("Custom", 413, 622);
            printDoc.DefaultPageSettings.Margins = new Margins(0, 0, 0, 0);
            printDoc.OriginAtMargins = false;

            printDoc.PrintPage += (sender, arg) =>
            {
                using (Image i = Image.FromFile(localPath))
                {
                    arg.Graphics.DrawImage(i, arg.MarginBounds);
                }
            };

            printDoc.Print();
        }

        public void PrepareAllPhotosArchive(PhotoEvent lastEvent)
        {
            Logger SecondTaskLogger = LogManager.GetLogger("secondTaskFile");

            Regex dslrPathRegexPattern = new Regex(dslrCompletedPhotosDirName);
            Regex instagramPathRegexPattern = new Regex(InstagramHelper.Instance.InstagramDirName);
            IEnumerable<FileInfo> dslrFiles =
                new DirectoryInfo(DslrPhotoDirPath)
                    .GetFiles("*.*", SearchOption.AllDirectories)
                    .Where(f =>
                        dslrPathRegexPattern.IsMatch(f.FullName) &&
                        f.LastWriteTime > lastEvent.StartDateTime &&
                        f.LastWriteTime < lastEvent.EndDateTime);
            SecondTaskLogger.Info("Total DSLR files for archiving: {0}", dslrFiles.Count());

            IEnumerable<FileInfo> instagramFiles = new DirectoryInfo(InstagramHelper.Instance.InstagramDirName)
                    .GetFiles("*.*", SearchOption.AllDirectories)
                    .Where(f =>
                        instagramPathRegexPattern.IsMatch(f.FullName) &&
                        f.LastWriteTime > lastEvent.StartDateTime &&
                        f.LastWriteTime < lastEvent.EndDateTime);
            SecondTaskLogger.Info("Total instagram files for archiving: {0}", instagramFiles.Count());

            IEnumerable<FileInfo> files = dslrFiles.Concat(instagramFiles);
            SecondTaskLogger.Info("Total files for archiving: {0}", files.Count());

            var archivePath = Path.Combine("Archives", lastEvent.Id + ".zip");
            using (var fileStream = new FileStream(archivePath, FileMode.CreateNew))
            {
                using (var archive = new ZipArchive(fileStream, ZipArchiveMode.Create, true))
                {
                    foreach (var file in files)
                    {
                        SecondTaskLogger.Info("File {0} added to archive", file.Name);
                        var fileBytes = ImageToByte(Image.FromFile(file.FullName));
                        var zipArchiveEntry = archive.CreateEntry(file.Name, CompressionLevel.Optimal);
                        using (var zipStream = zipArchiveEntry.Open())
                            zipStream.Write(fileBytes, 0, fileBytes.Length);
                    }
                }
            }
            SecondTaskLogger.Info("Archive by path {0} created", archivePath);
        }

        public static byte[] ImageToByte(Image img)
        {
            ImageConverter converter = new ImageConverter();
            return (byte[])converter.ConvertTo(img, typeof(byte[]));
        }

        public void DeleteFile(string filePath, Logger logger)
        {
            try
            {
                File.Delete(filePath);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }



        //public void ResizeImages(List<string> localFiles)
        //{
        //    foreach (var localFile in localFiles)
        //    {
        //        try
        //        {
        //            Image image = Image.FromFile(localFile);
        //            if (image.Width != 640 || image.Height != 640)
        //            {
        //                LogManager.GetLogger("InstagramSearchLogger").Info($"Resize required: {localFile}");
        //                Bitmap resizedImage = ImageHelper.Instance.ResizeImage(image, 640, 640);
        //                image.Dispose();

        //                DeleteFile(localFile);

        //                resizedImage = ImageHelper.Instance.AddBorderToImage(resizedImage);

        //                LogManager.GetLogger("InstagramSearchLogger").Info("Try to save file with original name");
        //                resizedImage.Save(localFile, ImageFormat.Jpeg);
        //                LogManager.GetLogger("InstagramSearchLogger").Info("Saved!");
        //                resizedImage.Dispose();
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            LogManager.GetLogger("InstagramSearchLogger").Error(ex);
        //            throw;
        //        }
        //    }

        //}

        public bool ResizeImage(string filePath)
        {
            bool result = false;
            Logger instagramLogger = LogManager.GetLogger("InstagramSearchLogger");
            Image image = Image.FromFile(filePath);

            try
            {
                instagramLogger.Info($"Locked 2.1: {Instance.IsFileLocked(new FileInfo(filePath))}");
                instagramLogger.Info($"Locked 2.2: {Instance.IsFileLocked(new FileInfo(filePath))}");
                if (image.Width != 640 || image.Height != 640)
                {
                    LogManager.GetLogger("InstagramSearchLogger").Info($"Resize required: {filePath}, width: {image.Width}, height: {image.Height}");
                    instagramLogger.Info($"Locked 2.3: {Instance.IsFileLocked(new FileInfo(filePath))}");
                    Bitmap resizedImage = ImageHelper.Instance.ResizeImage(image, 640, 640);
                    instagramLogger.Info($"Locked 2.4: {Instance.IsFileLocked(new FileInfo(filePath))}");
                    image.Dispose();

                    DeleteFile(filePath, instagramLogger);

                    LogManager.GetLogger("InstagramSearchLogger").Info("Try to save file with original name");
                    instagramLogger.Info($"Locked 2.5: {Instance.IsFileLocked(new FileInfo(filePath))}");
                    new Bitmap(resizedImage).Save(filePath, ImageFormat.Jpeg);
                    instagramLogger.Info($"Locked 2.6: {Instance.IsFileLocked(new FileInfo(filePath))}");
                    //resizedImage.Save(filePath, ImageFormat.Jpeg);
                    LogManager.GetLogger("InstagramSearchLogger").Info("Saved!");
                    resizedImage.Dispose();
                    instagramLogger.Info($"Locked 2.7: {Instance.IsFileLocked(new FileInfo(filePath))}");
                    using (Image imageTmp = Image.FromFile(filePath))
                    {
                        instagramLogger.Info($"Locked 2.8: {Instance.IsFileLocked(new FileInfo(filePath))}");
                        if (imageTmp.Width == 640 && imageTmp.Height == 640)
                        {
                            result = true;
                        }
                    }
                    instagramLogger.Info($"Locked 2.9: {Instance.IsFileLocked(new FileInfo(filePath))}");
                }
                else
                {
                    result = true;
                }
            }
            catch (Exception ex)
            {
                LogManager.GetLogger("InstagramSearchLogger").Error(ex);
            }
            image.Dispose();
            return result;
        }

    }
}