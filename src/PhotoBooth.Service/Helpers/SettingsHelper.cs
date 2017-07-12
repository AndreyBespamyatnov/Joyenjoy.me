using System;
using System.Collections.Generic;
using NLog;
using PhotoBooth.Models;

namespace PhotoBooth.Service.Helpers
{
    public class SettingsHelper
    {
        private static SettingsHelper _instance;
        public static Logger Log = LogManager.GetCurrentClassLogger();

        private SettingsHelper() { }


        public static SettingsHelper Instance
        {
            get { return _instance ?? (_instance = new SettingsHelper()); }
        }

        public void WriteEventsToFile(List<PhotoEvent> events)
        {
            //throw new System.NotImplementedException();
        }

        public List<PhotoEvent> ReadEventsFromFile()
        {
            throw new NotImplementedException();
        }

        public PhotoEvent FetchCurrentEventFromFile()
        {
            //return null if no current event
            throw new NotImplementedException();
        }

        public string SetDslrPhotoDirectoryPath()
        {
            //parse _dslrSettingsFileLocation prop
            throw new NotImplementedException();
        }
    }
}