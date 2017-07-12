using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using PhotoBooth.BoothService.Helpers;
using PhotoBooth.Models;

namespace PhotoBooth.BoothService.Tasks
{
    public class PrintTask
    {
        static readonly Logger PrintTaskLogger = LogManager.GetLogger("PrintTaskLogger");
        internal static async Task Do(CancellationToken token, int delaySeconds)
        {
            if (!Directory.Exists("PrintQueue"))
            {
                Directory.CreateDirectory("PrintQueue");
            }
            while (!token.IsCancellationRequested)
            {
                try
                {
                    List<PrintQueue> currentBoothPrintPhotos = ContextHelper.Instance.GetCurrentBoothPrintPhotos();
                    if (currentBoothPrintPhotos != null && currentBoothPrintPhotos.Count > 0)
                    {
                        foreach (var currentPhotoBoothPrintElement in currentBoothPrintPhotos)
                        {
                            string localPath = String.Empty;

                            string blobUrl = currentPhotoBoothPrintElement.BlobPathToImage;
                            string guidRegex = "(.{8}-.{4}-.{4}-.{4}-.{12})";
                            string eventId = Regex.Matches(blobUrl, guidRegex)[0].Value;
                            string fileName = Path.GetFileName(blobUrl);
                            localPath = Path.Combine("PrintQueue", Path.GetFileName(blobUrl));
                            BlobHelper.Instance.DownloadPhotoFromBlob(eventId, fileName, localPath);

                            FilesHelper.Instance.PrintFile(localPath);

                            ContextHelper.Instance.RemovePrintQueueElement(currentPhotoBoothPrintElement);
                            FilesHelper.Instance.WaitForFile(localPath);
                            File.Delete(localPath); //only if print successfull and file printed
                            PrintTaskLogger.Info("File {0} printed", localPath);
                        }
                    }
                    else
                    {
                        PrintTaskLogger.Info("No photos to print");
                    }
                    
                }
                catch (Exception ex)
                {
                    PrintTaskLogger.Error(ex);
                }
                await Task.Delay(TimeSpan.FromSeconds(delaySeconds), token);
            }
        }
    }
}