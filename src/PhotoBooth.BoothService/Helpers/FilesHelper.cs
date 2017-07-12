using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading;
using NLog;
using PhotoBooth.Models;

namespace PhotoBooth.BoothService.Helpers
{
    public class FilesHelper
    {
        private static FilesHelper _instance;
        public static string DslrPhotoDirPath = ConfigurationManager.AppSettings["dslrPhotosDirPath"];
        public static string WorkDirPath = ConfigurationManager.AppSettings["serviceWorkDirPath"];

        private const string ScFileName = "ScProcessedFileNames.csv";
        private const string ThFileName = "ThProcessedFileNames.csv";

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
                    using (stream = fileInfo.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None))
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

        public List<string> GetAlreadyProcessedFiles(string tempFileName)
        {
            CreateFileIfNotExist(tempFileName);
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
            PrintDocument printDoc = new PrintDocument { PrintController = new StandardPrintController() };
            printDoc.DocumentName = Path.GetFileName(localPath);
            printDoc.PrintPage += (sender, args) =>
            {
                Image i = Image.FromFile(localPath);
                Rectangle m = args.MarginBounds;

                if (i.Width / (double)i.Height > m.Width / (double)m.Height) // image is wider
                {
                    m.Height = (int)(i.Height / (double)i.Width * m.Width);
                }
                else
                {
                    m.Width = (int)(i.Width / (double)i.Height * m.Height);
                }
                args.Graphics.DrawImage(i, m);
            };
            printDoc.Print();

            //using (var proc = new Process())
            //{
            //    proc.StartInfo.FileName = localPath;
            //    proc.StartInfo.Verb = "Print";
            //    proc.Start();
            //}
        }

        public void PrepareAllPhotosArchive(PhotoEvent lastEvent)
        {
            Logger SecondTaskLogger = LogManager.GetLogger("secondTaskFile");

            Regex pathRegex = new Regex("photos");
            IEnumerable<FileInfo> files =
                new DirectoryInfo(DslrPhotoDirPath)
                    .GetFiles("*.*", SearchOption.AllDirectories)
                    .Where(f =>
                        pathRegex.IsMatch(f.FullName) &&
                        f.LastWriteTime > lastEvent.StartDateTime &&
                        f.LastWriteTime < lastEvent.EndDateTime);
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
    }
}