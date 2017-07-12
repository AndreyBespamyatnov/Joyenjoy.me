using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using NLog;
using PhotoBooth.DAL;
using PhotoBooth.Models;

namespace PhotoBooth.BoothService.Helpers
{
    public class EventHelper
    {
        private static EventHelper _instance;
        public static Logger Log = LogManager.GetCurrentClassLogger();
        readonly Guid _boothGuid = Guid.Parse(ConfigurationManager.AppSettings["BoothId"]);
        readonly int _checkEventTimeIntervalInSeconds = int.Parse(ConfigurationManager.AppSettings["CheckEventTimeIntervalInSeconds"]);

        private readonly ContextHelper _contextHelper;
        private readonly SettingsHelper _settingsHelper;

        private EventHelper()
        {
            _contextHelper = ContextHelper.Instance;
            _settingsHelper = SettingsHelper.Instance;
        }

        public PhotoEvent GetCurrentEvent(Logger logger)
        {
            PhotoEvent currentBoothEventNow = null;
            try
            {
                using (var db = new PhotoBoothContext())
                {
                    IQueryable<PhotoEvent> currentBoothEvents = db.PhotoEvents.Where(pe => pe.PhotoBoothEntityId == _boothGuid);
                    foreach (var currentBoothEvent in currentBoothEvents)
                    {
                        if (currentBoothEvent.StartDateTime <= DateTime.Now && currentBoothEvent.EndDateTime >= DateTime.Now)
                        {
                            currentBoothEventNow = currentBoothEvent;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
            
            if (currentBoothEventNow != null)
            {
                logger.Info("Event {0} right now!", currentBoothEventNow.Id);
                Settings.CurrentEvent = currentBoothEventNow;
                Settings.LastEvent = currentBoothEventNow;
            }
            else
            {
                logger.Info("No new events...");
                Settings.CurrentEvent = null;
            }

            return currentBoothEventNow;
        }

        //public async void IsEventNow(CancellationToken token)
        //{
        //    while (!token.IsCancellationRequested)
        //    {
        //        Log.Info("Check new events...");
        //        PhotoEvent currentEvent;

        //        if (_contextHelper.IsDatabaseConnectionExist())
        //        {
        //            Log.Trace("IsEventNow: Database connection -> true");
        //            using (PhotoBoothContext context = new PhotoBoothContext())
        //            {
        //                currentEvent = context.PhotoEvents.FirstOrDefault(e =>
        //                    e.PhotoBoothEntityId == _boothGuid &&
        //                    e.StartDateTime < DateTime.Now &&
        //                    e.EndDateTime > DateTime.Now);
        //                if (currentEvent != null)
        //                {
        //                    Log.Info("Current event: {0}", currentEvent.Id);
        //                }
        //                Settings.CurrentEvent = currentEvent;
        //                //Settings.IsEventNow = currentEvent != null;
        //                //Settings.SaveChanges();
        //            }
        //        }
        //        else
        //        {
        //            Log.Trace("IsEventNow: Database connection -> false");
        //            currentEvent = _settingsHelper.FetchCurrentEventFromFile();
        //            Log.Info("Current event: {0}", currentEvent.Id);
        //            Settings.CurrentEvent = currentEvent;
        //            //Settings.IsEventNow = currentEvent != null;
        //            //Settings.SaveChanges();
        //        }

        //        await Task.Delay(TimeSpan.FromSeconds(_checkEventTimeIntervalInSeconds), token);
        //    }
        //}

        public static EventHelper Instance
        {
            get { return _instance ?? (_instance = new EventHelper()); }
        }

        public List<PhotoEvent> FetchCurrentBoothEvents()
        {
            List<PhotoEvent> currentBoothEvents;
            using (PhotoBoothContext context = new PhotoBoothContext())
            {
                currentBoothEvents = context.PhotoEvents.Where(e => e.PhotoBoothEntityId == _boothGuid && e.StartDateTime > DateTime.Now).ToList();
            }
            return currentBoothEvents;
        }
    }
}