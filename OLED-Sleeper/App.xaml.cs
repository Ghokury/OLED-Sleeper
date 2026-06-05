using System.Linq;
using System.Windows;
using OLED_Sleeper.Infrastructure;

namespace OLED_Sleeper
{
    /// <summary>
    /// WPF application class. Handles only WPF lifecycle events and delegates startup/shutdown to ApplicationBootstrapper.
    /// </summary>
    public partial class App : Application
    {
        private ApplicationBootstrapper? _bootstrapper;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            SessionEnding += App_SessionEnding;

            bool pauseRequested = e.Args.Contains("-p");
            bool resumeRequested = e.Args.Contains("-s");
            bool exitRequested = e.Args.Contains("-e");
            bool startHidden = e.Args.Contains("-h");

            bool blackoutMonitor1Requested = e.Args.Contains("-b1");
            bool endBlackoutMonitor1Requested = e.Args.Contains("-e1");
            bool blackoutMonitor2Requested = e.Args.Contains("-b2");
            bool endBlackoutMonitor2Requested = e.Args.Contains("-e2");

            _bootstrapper = new ApplicationBootstrapper();
            _bootstrapper.Initialize(
                requestPause: pauseRequested,
                requestResume: resumeRequested,
                requestExit: exitRequested,
                startHidden: startHidden,
                requestBlackoutMonitor1: blackoutMonitor1Requested,
                requestEndBlackoutMonitor1: endBlackoutMonitor1Requested,
                requestBlackoutMonitor2: blackoutMonitor2Requested,
                requestEndBlackoutMonitor2: endBlackoutMonitor2Requested);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _bootstrapper?.ShutdownApp();
            _bootstrapper?.Dispose();
            base.OnExit(e);
        }

        private void App_SessionEnding(object sender, SessionEndingCancelEventArgs e)
        {
            _bootstrapper?.ShutdownApp();
        }
    }
}