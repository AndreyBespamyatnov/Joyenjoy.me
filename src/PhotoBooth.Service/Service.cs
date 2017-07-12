using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using PhotoBooth.Service.Tasks;

namespace PhotoBooth.Service
{
    public partial class Service : ServiceBase
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public Service()
        {
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
            InitializeComponent();
        }
        protected override void OnStart(string[] args)
        {
            this.RequestAdditionalTime(30000);

            InitializeService();

            CancellationToken token = _cancellationTokenSource.Token;
            Task.Run(() => FirstTask.Do(token, 5));
            Task.Run(() => SecondTask.Do(token, 5));
            Task.Run(() => ThirdTask.Do(token, 10));
            Task.Run(() => BoothAvailabilityTask.Do(token, 10));
            Task.Run(() => PrintTask.Do(token, 10));
            Task.Run(() => ArchiveUploaderTask.Do(token, 10));
            Task.Run(() => InstagramSearchTask.Do(token, 60));
        }

        protected override void OnStop()
        {
            SayToAllLoggers("┍---------------------┑");
            SayToAllLoggers("│  Stop service       │");
            SayToAllLoggers("┕---------------------┙");
            StopTasks();
        }

        public void StopTasks()
        {
            _cancellationTokenSource.Cancel();
        }

        private void InitializeService()
        {
            SayToAllLoggers("┍---------------------┑");
            SayToAllLoggers("│  Start service      │");
            SayToAllLoggers("┕---------------------┙");
        }

        private void SayToAllLoggers(string text)
        {
            string[] loggers = {
                "firstTaskFile",
                "secondTaskFile",
                "thirdTaskFile",
                "BoothAvailabilityLogger",
                "PrintTaskLogger",
                "ArchiveUploaderLogger",
                "InstagramSearchLogger" };
            foreach (var logger in loggers)
            {
                LogManager.GetLogger(logger).Info(text);
            }
        }
    }
}
