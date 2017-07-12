using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using PhotoBooth.Models;
using PhotoBooth.Service.Helpers;

namespace PhotoBooth.Service.Tasks
{
    public class InstagramSearchTask
    {
        static readonly Logger InstagramSearchLogger = LogManager.GetLogger("InstagramSearchLogger");
        public static string WorkDirPath = ConfigurationManager.AppSettings["serviceWorkDirPath"];
        internal static async Task Do(CancellationToken token, int delaySeconds)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (Settings.CurrentEvent != null)
                    {
                        List<string> instagrammImages = InstagramHelper.Instance.GetImageLinksByHashTag(Settings.CurrentEvent.HashTag);
                        List<string> instagrammAlreadyProcessedImages = FilesHelper.Instance.InstagramGetAlreadyProcessedFiles();
                        //List<string> instagrammNotProcessedImages = instagrammImages.Select(f => Path.GetFileName(f)).Except(instagrammAlreadyProcessedImages).ToList();

                        IEnumerable<string> instagramFileNames = instagrammImages.Select(Path.GetFileName);
                        IEnumerable<string> processedFileNames = instagrammAlreadyProcessedImages.Select(Path.GetFileName);
                        IEnumerable<string> diff = instagramFileNames.Except(processedFileNames);
                        List<string> instagrammNotProcessedImages = (from instagramImage in instagrammImages from df in diff where instagramImage.Contains(df) select instagramImage).ToList();

                        InstagramHelper.Instance.PrepareImages(instagrammNotProcessedImages);

                        //InstagramHelper.Instance.DownloadImages(instagrammNotProcessedImages);

                        List<string> localFiles = InstagramHelper.Instance.GetInstagramLocalFiles();
                        InstagramSearchLogger.Info($"Local files found: {localFiles.Count}");

                        foreach (var notProcessedFile in localFiles)
                        {
                            if (ContextHelper.Instance.IsInternetConnectionExist())
                            {
                                string fileMd5 = FilesHelper.Instance.CalculateMd5(notProcessedFile);
                                if (!instagrammAlreadyProcessedImages.Contains(notProcessedFile) && !ContextHelper.Instance.IsPhotoRecordExist(notProcessedFile, InstagramSearchLogger))
                                {
                                    FilesHelper.Instance.WaitForFile(notProcessedFile);

                                    #region preview

                                    string filePreviewLocalPath = ImageHelper.Instance.GetImageThumbnail(notProcessedFile);
                                    string filePreviewStorageUri = BlobHelper.Instance.UploadFileToContainer(filePreviewLocalPath,
                                            Settings.CurrentEvent.Id.ToString(), InstagramSearchLogger);
                                    FilesHelper.Instance.DeleteFile(filePreviewLocalPath, InstagramSearchLogger);

                                    #endregion

                                    #region photo

                                    string fileStorageUri = BlobHelper.Instance.UploadFileToContainer(notProcessedFile, Settings.CurrentEvent.Id.ToString(), InstagramSearchLogger);

                                    Photo photo = new Photo()
                                    {
                                        Id = Guid.NewGuid(),
                                        BlobPathToImage = fileStorageUri,
                                        PhotoEventId = Settings.CurrentEvent.Id,
                                        LocalPathToImage = notProcessedFile,
                                        BlobPathToPreviewImage = filePreviewStorageUri,
                                        Md5Hash = fileMd5,
                                        ImageHeight = FilesHelper.Instance.GetImageHeight(notProcessedFile),
                                        ImageWidth = FilesHelper.Instance.GetImageWidth(notProcessedFile)
                                    };
                                    bool isDbRecordCreated = ContextHelper.Instance.AddPhoto(photo, InstagramSearchLogger);

                                    #endregion

                                    if (string.IsNullOrEmpty(fileStorageUri) || !isDbRecordCreated)
                                    {
                                        //all bad
                                    }
                                    else
                                    {
                                        //all good!
                                        FilesHelper.Instance.InstagramAddProcessedFile(Path.GetFileName(notProcessedFile));
                                        //FilesHelper.Instance.DeleteFile(notProcessedFile, InstagramSearchLogger);
                                        InstagramSearchLogger.Info("All good!");
                                    }
                                }
                                else //db record exit
                                {
                                    FilesHelper.Instance.InstagramAddProcessedFile(Path.GetFileName(notProcessedFile));
                                }
                            }
                            else //no internet connection
                            {
                                if (!instagrammAlreadyProcessedImages.Contains(notProcessedFile))
                                {
                                    string eventPath = Path.Combine(WorkDirPath, Settings.CurrentEvent.Id.ToString());
                                    if (!Directory.Exists(eventPath))
                                    {
                                        Directory.CreateDirectory(eventPath);
                                    }
                                    string targetFilePath = Path.Combine(eventPath, Path.GetFileName(notProcessedFile));
                                    FilesHelper.Instance.WaitForFile(notProcessedFile);
                                    File.Copy(notProcessedFile, targetFilePath, true);

                                    FilesHelper.Instance.InstagramAddProcessedFile(Path.GetFileName(notProcessedFile));
                                    InstagramSearchLogger.Info("File {0} copied to workdir", targetFilePath);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    InstagramSearchLogger.Error(ex);
                }
                await Task.Delay(TimeSpan.FromSeconds(delaySeconds), token);
            }
        }
    }
}