using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using PhotoBooth.Service.Helpers;

namespace PhotoBooth.Service.Tasks
{
    public class ArchiveUploaderTask
    {
        static readonly Logger ArchiveUploaderLogger = LogManager.GetLogger("ArchiveUploaderLogger");
        internal static async Task Do(CancellationToken token, int delaySeconds)
        {
            string archivesDirPath = "Archives";
            if (!Directory.Exists(archivesDirPath))
            {
                Directory.CreateDirectory(archivesDirPath);
            }
            while (!token.IsCancellationRequested)
            {
                try
                {
                    string[] files = Directory.GetFiles(archivesDirPath, "*.zip");
                    ArchiveUploaderLogger.Info("Archives ready to upload: {0}", files.Count());

                    foreach (var file in files)
                    {
                        ArchiveUploaderLogger.Info("Archive {0} upload started", file);
                        FilesHelper.Instance.WaitForFile(file);
                        string blobUrl = BlobHelper.Instance.UploadFileToContainer(file, Path.GetFileNameWithoutExtension(file), ArchiveUploaderLogger);
                        if (!string.IsNullOrEmpty(blobUrl))
                        {
                            ArchiveUploaderLogger.Info("Archive {0} successfully uploaded, blob url: {1}", file, blobUrl);
                            File.Delete(file);
                        }
                        else
                        {
                            ArchiveUploaderLogger.Info("blobUrl is empty! Alarm!");
                        }
                    }
                }
                catch (Exception ex)
                {
                    ArchiveUploaderLogger.Error(ex);
                }

                await Task.Delay(TimeSpan.FromSeconds(delaySeconds), token);
            }
        }
    }
}