using System;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using PhotoBooth.Service.Helpers;

namespace PhotoBooth.Service.Tasks
{
    public class BoothAvailabilityTask
    {
        static readonly Logger BoothAvailabilityTaskLogger = LogManager.GetLogger("BoothAvailabilityLogger");
        internal static async Task Do(CancellationToken token, int delaySeconds)
        {
            while (!token.IsCancellationRequested)
            {
                bool result = ContextHelper.Instance.SetBoothAvailable();
                BoothAvailabilityTaskLogger.Info("Boot available: {0}", result);
                await Task.Delay(TimeSpan.FromSeconds(delaySeconds), token);
            }
        }
    }
}