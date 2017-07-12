using System;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using PhotoBooth.BoothService.Helpers;
using PhotoBooth.Models;

namespace PhotoBooth.BoothService.Tasks
{
    public class FirstTask
    {
        static readonly Logger FirstTaskLogger = LogManager.GetLogger("firstTaskFile");
        private static PhotoEvent _currentEvent;
        internal static async Task Do(CancellationToken token, int delaySeconds)
        {
            while (!token.IsCancellationRequested)
            {
                if (ContextHelper.Instance.IsDatabaseConnectionExist()) //todo: add timeout if very slow internet
                {
                    EventHelper.Instance.GetCurrentEvent(FirstTaskLogger);
                }
                else
                {
                    FirstTaskLogger.Info("No internet connection, need to read settings from local resource.");
                }

                await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
            }
        }
    }
}