using System.Collections.Generic;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using PhotoBooth.BoothService.Helpers;
using PhotoBooth.BoothService.Tasks;

namespace PhotoBooth.BoothService
{
    public partial class PhotoBoothService : ServiceBase
    {
        //public static Logger Log;
        private readonly CancellationTokenSource _cancellationTokenSource;
        //private Task _isEventNow;
        //private Task _isNewPhotoExist;
        //private readonly EventHelper _eventHelper;
        //private readonly ImageHelper _imageHelper;
        //private readonly BlobHelper _blobHelper;
        //private readonly ContextHelper _contextHelper;
        //private readonly SettingsHelper _settingsHelper;

        public PhotoBoothService()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            InitializeComponent();
        }

        public void OnDebug()
        {
            OnStart(null);
        }

        protected override void OnStart(string[] args)
        {
            InitializeService();

            CancellationToken token = _cancellationTokenSource.Token;

            Task.Run(() => FirstTask.Do(token, 5));
            Task.Run(() => SecondTask.Do(token, 5));
            Task.Run(() => ThirdTask.Do(token, 10));
            Task.Run(() => BoothAvailabilityTask.Do(token, 10));
            Task.Run(() => PrintTask.Do(token, 10));
            Task.Run(() => ArchiveUploaderTask.Do(token, 10));

            while (!token.IsCancellationRequested) {}
        }

        private void InitializeService()
        {
            string[] loggers = { "firstTaskFile", "secondTaskFile", "thirdTaskFile", "BoothAvailabilityLogger", "PrintTaskLogger", "ArchiveUploaderLogger" };
            foreach (var logger in loggers)
            {
                LogManager.GetLogger(logger).Info("┍---------------------┑");
                LogManager.GetLogger(logger).Info("│   Start new session  │");
                LogManager.GetLogger(logger).Info("┕---------------------┙");
            }

            //if (_contextHelper.IsDatabaseConnectionExist())
            //{
            //    Log.Trace("IsDatabaseConnectionExist: true");
            //    Settings.Events = _eventHelper.FetchCurrentBoothEvents();
            //    Settings.SaveChanges();
            //    //_settingsHelper.WriteEventsToFile(Settings.Events);
            //}
            //else
            //{
            //    Log.Trace("IsDatabaseConnectionExist: false");
            //    Settings.Events = _settingsHelper.ReadEventsFromFile();
            //    Settings.SaveChanges();
            //}
        }

        protected override void OnStop()
        {
            string[] loggers = { "firstTaskFile", "secondTaskFile", "thirdTaskFile" };
            foreach (var logger in loggers)
            {
                LogManager.GetLogger(logger).Info("┍---------------------┑");
                LogManager.GetLogger(logger).Info("│      Stop service    │");
                LogManager.GetLogger(logger).Info("┕---------------------┙");
            }
            StopTasks();
        }

        public void StopTasks()
        {
            //Log.Info("Stop tasks...");
            //if (_isEventNow != null || _isNewPhotoExist != null)
            //{
                _cancellationTokenSource.Cancel();
                //List<Task> tasks = new List<Task>();

                //if (_isEventNow != null)
                //    tasks.Add(_isEventNow);
                //if (_isNewPhotoExist != null)
                //    tasks.Add(_isNewPhotoExist);

                //Task[] tasksArray = tasks.ToArray();
                //Task.WaitAll(tasksArray);

                //_isEventNow = null;
                //_isNewPhotoExist = null;
                //Log.Info("Tasks stopped.");
            //}
        }

    }
}
