using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using PhotoBooth.Models;
using PhotoBooth.Service.Helpers;

namespace PhotoBooth.Service.Tasks
{
    public class FirstTask
    {
        static readonly Logger FirstTaskLogger = LogManager.GetLogger("firstTaskFile");
        private static PhotoEvent _currentEvent;
        internal static async Task Do(CancellationToken token, int delaySeconds)
        {
            while (!token.IsCancellationRequested)
            {
                EventInfo eventInfo;

                PhotoEvent currentEvent = EventHelper.Instance.GetCurrentEvent();
                if (currentEvent != null)
                {
                    FirstTaskLogger.Info("Current event id: {0}", currentEvent.Id);
                    if (currentEvent.InstagrammBrandingImage == null)
                    {
                        Settings.BrandImage = InstagramHelper.Instance.DefaultBrandingImage;
                        FirstTaskLogger.Info("Branding is default");
                    }
                    else
                    {
                        if (Settings.BrandImage == null)
                        {
                            Settings.BrandImage = currentEvent.InstagrammBrandingImage;
                            FirstTaskLogger.Info("Branding is custom");
                        }
                    }
                    Settings.CurrentEvent = currentEvent;
                    Settings.LastEvent = currentEvent; //for archive preparation
                    eventInfo = new EventInfo()
                    {
                        Id = currentEvent.Id,
                        StartDate = currentEvent.StartDateTime,
                        EndDate = currentEvent.EndDateTime,
                        HashTag = currentEvent.HashTag,
                        InstagrammBrandingImage = currentEvent.InstagrammBrandingImage
                    };
                    EventHelper.Instance.WriteEventToFile(eventInfo);
                }
                else
                {
                    FirstTaskLogger.Info("Current event in db == null");

                    eventInfo = EventHelper.Instance.ReadEventFromFile();
                    if (eventInfo != null )
                    {
                        FirstTaskLogger.Info("Local file contain event, id: {0}", eventInfo.Id);

                        if (eventInfo.InstagrammBrandingImage == null)
                        {
                            Settings.BrandImage = InstagramHelper.Instance.DefaultBrandingImage;
                            FirstTaskLogger.Info("local: Branding is default");
                        }
                        else
                        {
                            if (Settings.BrandImage == null)
                            {
                                Settings.BrandImage = eventInfo.InstagrammBrandingImage;
                                FirstTaskLogger.Info("local: Branding is custom");
                            }
                        }

                        PhotoEvent photoEvent = new PhotoEvent() {
                            Id = eventInfo.Id,
                            StartDateTime = eventInfo.StartDate,
                            EndDateTime = eventInfo.EndDate,
                            HashTag = eventInfo.HashTag,
                            InstagrammBrandingImage = eventInfo.InstagrammBrandingImage};
                        Settings.CurrentEvent = photoEvent;
                        Settings.LastEvent = photoEvent; //for archive preparation
                    }
                    else
                    {
                        FirstTaskLogger.Info("Local file not exits or no event now");
                        EventHelper.Instance.DeleteEventInfoFile();
                        Settings.BrandImage = null;
                        Settings.CurrentEvent = null;
                    }
                }
                await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
            }
        }
    }
}