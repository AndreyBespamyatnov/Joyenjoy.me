using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using PhotoBooth.BoothService.Helpers;
using PhotoBooth.Models;

namespace PhotoBooth.BoothService.Tasks
{
    public class SecondTask
    {
        static readonly Logger SecondTaskLogger = LogManager.GetLogger("secondTaskFile");
        public static string DslrPhotoDirPath = ConfigurationManager.AppSettings["dslrPhotosDirPath"];
        public static string WorkDirPath = ConfigurationManager.AppSettings["serviceWorkDirPath"];

        internal static async Task Do(CancellationToken token, int delaySeconds)
        {
            int eventEndCounter = 0;
            while (!token.IsCancellationRequested)
            {
                try
                {
                    List<string> alreadyProcessedFiles = FilesHelper.Instance.ScGetAlreadyProcessedFiles();

                    if (Settings.CurrentEvent != null)
                    {
                        

                        Regex pathRegex = new Regex("photos");
                        IEnumerable<FileInfo> localFilesInEventDateRange =
                            new DirectoryInfo(DslrPhotoDirPath)
                            .GetFiles("*.*", SearchOption.AllDirectories)
                            .Where(f =>
                                pathRegex.IsMatch(f.FullName) &&
                                f.LastWriteTime > Settings.CurrentEvent.StartDateTime &&
                                f.LastWriteTime < Settings.CurrentEvent.EndDateTime);
                        IEnumerable<string> notProcessedFiles = localFilesInEventDateRange.Select(f => f.FullName).Except(alreadyProcessedFiles);
                        foreach (var notProcessedFile in notProcessedFiles)
                        {
                            if (ContextHelper.Instance.IsInternetConnectionExist())
                            {
                                string fileMd5 = FilesHelper.Instance.CalculateMd5(notProcessedFile);
                                if (!alreadyProcessedFiles.Contains(Path.GetFileName(notProcessedFile)) && !ContextHelper.Instance.IsPhotoRecordExist(notProcessedFile, SecondTaskLogger))
                                {
                                    FilesHelper.Instance.WaitForFile(notProcessedFile);

                                    #region preview file

                                    #region upload preview file

                                    string filePreviewLocalPath = ImageHelper.Instance.GetImageThumbnail(notProcessedFile);
                                    string filePreviewStorageUri = BlobHelper.Instance.UploadFileToContainer(filePreviewLocalPath,
                                            Settings.CurrentEvent.Id.ToString(), SecondTaskLogger);

                                    #endregion

                                    #region creare db record for preview file

                                    //Photo photoPreview = new Photo()
                                    //{
                                    //    Id = Guid.NewGuid(),
                                    //    PhotoEventId = Settings.CurrentEvent.Id,
                                    //    BlobPathToImage = filePreviewStorageUri,
                                    //    LocalPathToImage = filePreviewLocalPath,
                                    //    Md5Hash = FilesHelper.Instance.CalculateMd5(filePreviewLocalPath)
                                    //};
                                    //bool previewFileRecordResult = ContextHelper.Instance.AddPhoto(photoPreview, SecondTaskLogger);

                                    File.Delete(filePreviewLocalPath);

                                    #endregion

                                    #endregion

                                    #region main file

                                    #region upload main file

                                    string fileStorageUri = BlobHelper.Instance.UploadFileToContainer(notProcessedFile,
                                        Settings.CurrentEvent.Id.ToString(), SecondTaskLogger);

                                    #endregion

                                    #region create db record for main file

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
                                    bool isDbRecordCreated = ContextHelper.Instance.AddPhoto(photo, SecondTaskLogger);

                                    if (string.IsNullOrEmpty(fileStorageUri) || !isDbRecordCreated)
                                    {
                                        //all bad
                                    }
                                    else
                                    {
                                        //all good!
                                        FilesHelper.Instance.ScAddProcessedFile(Path.GetFileName(notProcessedFile));
                                        SecondTaskLogger.Info("All good!");
                                    }

                                    #endregion

                                    #endregion
                                }
                                else //db record exit
                                {
                                    FilesHelper.Instance.ScAddProcessedFile(Path.GetFileName(notProcessedFile));
                                    //SecondTaskLogger.Info("File {0} with md5 {1} already exist in database", Path.GetFileName(notProcessedFile), fileMd5);
                                }
                            }
                            else //no internet connection
                            {
                                if (!alreadyProcessedFiles.Contains(Path.GetFileName(notProcessedFile)))
                                {
                                    string eventPath = Path.Combine(WorkDirPath, Settings.CurrentEvent.Id.ToString());
                                    if (!Directory.Exists(eventPath))
                                    {
                                        Directory.CreateDirectory(eventPath);
                                    }
                                    string targetFilePath = Path.Combine(eventPath, Path.GetFileName(notProcessedFile));
                                    FilesHelper.Instance.WaitForFile(notProcessedFile);
                                    File.Copy(notProcessedFile, targetFilePath, true);

                                    FilesHelper.Instance.ScAddProcessedFile(Path.GetFileName(notProcessedFile));
                                    SecondTaskLogger.Info("File {0} copied to workdir", targetFilePath);
                                }
                            }
                        }
                    }
                    if (Settings.CurrentEvent == null && Settings.LastEvent != null)
                    {
                        eventEndCounter++;
                    }
                    if (eventEndCounter >= 5 && Settings.LastEvent != null)
                    {
                        SecondTaskLogger.Info("Lets prepare all photos archive");
                        FilesHelper.Instance.PrepareAllPhotosArchive(Settings.LastEvent);
                        eventEndCounter = 0;
                        Settings.LastEvent = null;
                    }
                }
                catch (Exception ex)
                {
                    SecondTaskLogger.Error(ex);
                }

                await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
            }
        }
    }
}