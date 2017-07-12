using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using NLog;
using PhotoBooth.DAL;
using PhotoBooth.Models;

namespace PhotoBooth.Service.Helpers
{
    public class EventHelper
    {
        private static EventHelper _instance;
        public static Logger Log = LogManager.GetCurrentClassLogger();
        readonly Guid _boothGuid = Guid.Parse(ConfigurationManager.AppSettings["BoothId"]);

        public PhotoEvent GetCurrentEvent()
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
                LogManager.GetLogger("firstTaskFile").Error(ex);
            }

            //if (currentBoothEventNow != null)
            //{
            //    logger.Info("Event {0} right now!", currentBoothEventNow.Id);
            //    Settings.CurrentEvent = currentBoothEventNow;
            //    Settings.LastEvent = currentBoothEventNow;

            //    EventInfo eventInfo = new EventInfo()
            //    {
            //        Id = currentBoothEventNow.Id,
            //        StartDate = currentBoothEventNow.StartDateTime,
            //        EndDate = currentBoothEventNow.EndDateTime
            //    };
            //    WriteEventToFile(eventInfo);
            //}
            //else
            //{
            //    logger.Info("No new events...");
            //    Settings.CurrentEvent = null;
            //}

            return currentBoothEventNow;
        }

        public void WriteEventToFile(EventInfo eventInfo)
        {
            Logger logger = LogManager.GetLogger("firstTaskFile");

            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(EventInfo));
                StringWriter stringWriter = new StringWriter();
                xmlSerializer.Serialize(stringWriter, eventInfo);
                string xml = stringWriter.ToString();
                File.WriteAllText("OfflineEventInfo.xml", xml);
                logger.Info("File with offline event data stored");
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        public EventInfo ReadEventFromFile()
        {
            Logger logger = LogManager.GetLogger("firstTaskFile");
            EventInfo eventInfo = null;
            try
            {
                if (File.Exists("OfflineEventInfo.xml"))
                {
                    string fileContent = File.ReadAllText("OfflineEventInfo.xml");
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(EventInfo));
                    StringReader stringReader = new StringReader(fileContent);
                    EventInfo eventInfoData = (EventInfo)xmlSerializer.Deserialize(stringReader);
                    if (eventInfoData.StartDate <= DateTime.Now && eventInfoData.EndDate >= DateTime.Now)
                    {
                        eventInfo = eventInfoData;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
            return eventInfo;
        }

        public void DeleteEventInfoFile()
        {
            Logger logger = LogManager.GetLogger("firstTaskFile");
            string fileName = "OfflineEventInfo.xml";
            if (File.Exists(fileName))
            {
                try
                {
                    File.Delete("fileName");
                    logger.Info("OfflineEventInfo.xml deleted");
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                }
            }
        }

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

    public class EventInfo
    {
        public Guid Id { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string HashTag { get; set; }
        public string InstagrammBrandingImage { get; set; }
    }

}