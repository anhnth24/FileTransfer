
using System;
using System.Configuration;
using System.ServiceProcess;
using System.Timers;

namespace FileTransfer
{
    public partial class Service : ServiceBase
    {
        private static Timer _timer;
        private static readonly int TIME_RESTART = int.Parse(ConfigurationManager.AppSettings["TimeRestart"].ToString());
        public Service()
        {
            InitializeComponent();
        }

        public void OnDebug()
        {
            ServiceFileTransfer.SendFileUpload();
            ServiceFileTransfer.DownFileUpload();
            ServiceFileTransfer.SendFileTemplate();
            ServiceFileTransfer.DownFileTemplate();
        }

        protected override void OnStart(string[] args)
        {
            base.OnStart(args);
            _timer = new Timer();
            _timer.AutoReset = false;
            _timer.Elapsed += OnTimer;
            _timer.Start();

        }

        private void OnTimer(object sender, ElapsedEventArgs args)
        {
            try
            {
                ServiceFileTransfer.SendFileUpload();
                ServiceFileTransfer.DownFileUpload();
                ServiceFileTransfer.SendFileTemplate();
                ServiceFileTransfer.DownFileTemplate();
            }
            catch (Exception)
            {

            }
            _timer.Stop();
            _timer.Interval = TIME_RESTART; //Service time
            _timer.Start();
        }
        protected override void OnStop()
        {
            base.OnStop(); 
        }

        protected override void OnShutdown()
        {
            base.OnShutdown();
        }
    }
}
