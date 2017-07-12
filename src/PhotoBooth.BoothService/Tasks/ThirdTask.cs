using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using PhotoBooth.BoothService.Helpers;
using PhotoBooth.Models;

namespace PhotoBooth.BoothService.Tasks
{
    public class ThirdTask
    {
        static readonly Logger ThirdTaskLogger = LogManager.GetLogger("thirdTaskFile");
        public static string WorkDirPath = ConfigurationManager.AppSettings["serviceWorkDirPath"];
        internal static async Task Do(CancellationToken token, int delaySeconds)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    List<string> workDirFiles = FilesHelper.Instance.GetWorkDirFiles();
                    ThirdTaskLogger.Info("{0} files want to be uploaded", workDirFiles.Count);

                    foreach (var fullFilePath in workDirFiles)
                    {
                        string eventId = FilesHelper.Instance.GetEventIdFromPath(fullFilePath);
                        string fileMd5 = FilesHelper.Instance.CalculateMd5(fullFilePath);

                        if (!FilesHelper.Instance.ThGetAlreadyProcessedFiles().Contains(fullFilePath))
                        {
                            if (ContextHelper.Instance.IsInternetConnectionExist())
                            {
                                if (!FilesHelper.Instance.ThGetAlreadyProcessedFiles().Contains(Path.GetFileName(Path.GetFileName(fullFilePath))) &&
                                    !ContextHelper.Instance.IsPhotoRecordExist(fullFilePath, ThirdTaskLogger))
                                {
                                    FilesHelper.Instance.WaitForFile(fullFilePath);

                                    #region preview file

                                    #region upload preview file

                                    string filePreviewLocalPath = ImageHelper.Instance.GetImageThumbnail(fullFilePath);
                                    string filePreviewStorageUri =
                                        BlobHelper.Instance.UploadFileToContainer(filePreviewLocalPath, eventId,
                                            ThirdTaskLogger);
                                    File.Delete(filePreviewLocalPath);

                                    #endregion

                                    #endregion

                                    #region main file

                                    #region upload main file

                                    string fileStorageUri = BlobHelper.Instance.UploadFileToContainer(fullFilePath, eventId,
                                        ThirdTaskLogger);

                                    #endregion

                                    #region create db record for main file

                                    Photo photo = new Photo()
                                    {
                                        Id = Guid.NewGuid(),
                                        BlobPathToImage = fileStorageUri,
                                        PhotoEventId = Guid.Parse(eventId),
                                        LocalPathToImage = fullFilePath,
                                        BlobPathToPreviewImage = filePreviewStorageUri,
                                        Md5Hash = FilesHelper.Instance.CalculateMd5(fullFilePath),
                                        ImageHeight = FilesHelper.Instance.GetImageHeight(fullFilePath),
                                        ImageWidth = FilesHelper.Instance.GetImageWidth(fullFilePath)
                                    };
                                    bool isDbRecordCreated = ContextHelper.Instance.AddPhoto(photo, ThirdTaskLogger);

                                    #endregion

                                    #endregion

                                    if (string.IsNullOrEmpty(fileStorageUri) || !isDbRecordCreated)
                                    {
                                        ThirdTaskLogger.Error("All bad!");
                                    }
                                    else
                                    {
                                        FilesHelper.Instance.WaitForFile(fullFilePath);
                                        File.Delete(fullFilePath);
                                        FilesHelper.Instance.ThAddProcessedFile(Path.GetFileName(fullFilePath));
                                        ThirdTaskLogger.Info("All good!");
                                    }
                                }
                                else
                                {
                                    ThirdTaskLogger.Info("File with md5 {0} already exist in database", fileMd5);
                                    File.Delete(fullFilePath);
                                }

                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ThirdTaskLogger.Error(ex);
                }

                await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
            }
        }

    }
}