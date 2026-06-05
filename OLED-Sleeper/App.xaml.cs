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

            _bootstrapper = new ApplicationBootstrapper();
            _bootstrapper.Initialize(
                requestPause: pauseRequested,
                requestResume: resumeRequested,
                requestExit: exitRequested);
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