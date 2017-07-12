using System;
using System.Collections.Generic;
using PhotoBooth.Models;

namespace PhotoBooth.BoothService
{
    public static class Settings
    {
        //public static Guid BoothId { get; set; }
        //public static bool IsEventNow { get; set; }
        //public static bool EventStartTime { get; set; }
        //public static bool EventEndTime { get; set; }
        //public static List<PhotoEvent> Events { get; set; }
        public static PhotoEvent CurrentEvent { get; set; }
        public static PhotoEvent LastEvent { get; set; }
        //public static string DslrPhotoDirectoryPath { get; set; }

        //static Settings()
        //{
        //    Events = new List<PhotoEvent>();
        //    DslrPhotoDirectoryPath = @"C:\DSLR_Images\2015-07-05"; //TODO: remove this
        //}

        //public static void SaveChanges()
        //{

        //}
    }
}