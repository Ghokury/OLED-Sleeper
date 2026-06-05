using Microsoft.Extensions.DependencyInjection;
using OLED_Sleeper.Core;
using OLED_Sleeper.Core.Interfaces;
using OLED_Sleeper.UI.Services.Interfaces;
using Serilog;
using System.Windows;

namespace OLED_Sleeper.Infrastructure
{
    /// <summary>
    /// Handles application startup, dependency injection, single-instance enforcement, orchestrator startup, and shutdown logic.
    /// </summary>
    public class ApplicationBootstrapper : IDisposable
    {
        private IServiceProvider? _serviceProvider;
        private ITrayIconService? _trayIconService;
        private IMainWindowService? _mainWindowService;
        private ApplicationInstanceManager? _instanceManager;
        private bool _isExiting = false;

        public void Initialize(bool requestPause = false, bool requestResume = false, bool requestExit = false, bool startHidden = false)
        {
            LoggingConfigurator.Configure();
            InitializeInstanceManager(requestPause, requestResume, requestExit);

            if (_instanceManager is { IsFirstInstance: false })
                return;

            ConfigureServices(startHidden);
            StartOrchestrator();
            SetupMainWindowService();
            SetupTrayIconService();
            HookInstanceManagerActions();
        }

        private void InitializeInstanceManager(bool requestPause, bool requestResume, bool requestExit)
        {
            _instanceManager = new ApplicationInstanceManager();
            _instanceManager.Initialize(requestPause, requestResume, requestExit);
        }

        private void ConfigureServices(bool startHidden = false)
        {
            var applicationOptions = new ApplicationOptions
            {
                StartPaused = false,
                StartHidden = startHidden,
                ExitImmediately = false,
                PauseImmediately = false,
                ResumeImmediately = false,
                StartMinimized = false,
                TrayOnly = false
            };

            _serviceProvider = ServiceConfigurator.ConfigureServices(_instanceManager!, applicationOptions);
        }

        private void StartOrchestrator()
        {
            if (_serviceProvider == null)
                return;

            var orchestrator = _serviceProvider.GetRequiredService<IApplicationOrchestrator>();
            orchestrator.Start();
        }

        private void SetupMainWindowService()
        {
            if (_serviceProvider == null) return;
            _mainWindowService = _serviceProvider.GetRequiredService<IMainWindowService>();
            _mainWindowService.SetupMainWindow();
        }

        private void SetupTrayIconService()
        {
            if (_serviceProvider == null) return;
            _trayIconService = _serviceProvider.GetRequiredService<ITrayIconService>();
            _trayIconService.Initialize(
                () => _mainWindowService?.ShowMainWindow(),
                () => ShutdownApp()
            );
        }

        private void HookInstanceManagerActions()
        {
            if (_instanceManager == null || _serviceProvider == null)
                return;

            var orchestrator = _serviceProvider.GetRequiredService<IApplicationOrchestrator>();

            _instanceManager.SetShowMainWindowAction(() => _mainWindowService?.ShowMainWindow());
            _instanceManager.SetPauseAction(() => orchestrator.Pause());
            _instanceManager.SetResumeAction(() => orchestrator.Resume());
            _instanceManager.SetExitAction(() => ShutdownApp());
        }

        public void ShutdownApp()
        {
            if (_isExiting) return;
            _isExiting = true;

            if (_instanceManager?.IsFirstInstance == true)
            {
                Log.Information("Shutdown initiated. Restoring all monitors...");
                ApplicationNotifications.TriggerRestoreAllMonitors();
            }

            Log.Information("--- Application Exiting ---");
            Log.CloseAndFlush();

            _trayIconService?.Dispose();
            _instanceManager?.Dispose();
            Application.Current.Shutdown();
        }

        public void Dispose()
        {
            _trayIconService?.Dispose();
            _instanceManager?.Dispose();
            (_serviceProvider as IDisposable)?.Dispose();
        }
    }
}
