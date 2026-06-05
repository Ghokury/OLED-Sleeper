using OLED_Sleeper.Core.Interfaces;
using OLED_Sleeper.Features.MonitorBehavior.Commands;
using OLED_Sleeper.Features.MonitorDimming.Commands;
using OLED_Sleeper.Features.MonitorIdleDetection.Services.Interfaces;
using OLED_Sleeper.Features.MonitorState.Services.Interfaces;
using OLED_Sleeper.Features.UserSettings.Models;
using OLED_Sleeper.Features.UserSettings.Services.Interfaces;
using Serilog;

namespace OLED_Sleeper.Core
{
    /// <summary>
    /// Central application orchestrator for monitor management in OLED-Sleeper.
    /// </summary>
    public class ApplicationOrchestrator : IApplicationOrchestrator
    {
        private readonly IMediator _mediator;
        private readonly IMonitorIdleDetectionService _monitorIdleDetectionService;
        private readonly IMonitorSettingsFileService _monitorSettingsFileService;
        private readonly IMonitorStateWatcher _monitorStateWatcher;

        private bool _isStarted;
        private bool _isPaused;

        public ApplicationOrchestrator(
            IMediator mediator,
            IMonitorIdleDetectionService monitorIdleDetectionService,
            IMonitorSettingsFileService monitorSettingsFileService,
            IMonitorStateWatcher monitorStateWatcher)
        {
            _mediator = mediator;
            _monitorIdleDetectionService = monitorIdleDetectionService;
            _monitorSettingsFileService = monitorSettingsFileService;
            _monitorStateWatcher = monitorStateWatcher;
        }

        public void Start()
        {
            if (_isStarted)
                return;

            SendRestoreBrightnessOnAllMonitorsCommand();

            var settings = _monitorSettingsFileService.LoadSettings();
            _monitorIdleDetectionService.UpdateSettings(settings);
            _monitorIdleDetectionService.Start();

            SubscribeToEvents();
            InitializeStateWatcher();

            _isStarted = true;
            _isPaused = false;

            Log.Information("ApplicationOrchestrator started.");
        }

        public void Stop()
        {
            if (!_isStarted)
                return;

            Log.Information("ApplicationOrchestrator is stopping.");
            RestoreAllMonitors();
            UnsubscribeFromEvents();
            _monitorIdleDetectionService.Stop();
            _monitorStateWatcher.Stop();

            _isStarted = false;
            _isPaused = false;
        }

        public void Pause()
        {
            if (!_isStarted || _isPaused)
                return;

            Log.Information("ApplicationOrchestrator paused.");

            _monitorIdleDetectionService.Stop();
            RestoreAllMonitors();

            var settings = _monitorSettingsFileService.LoadSettings();
            foreach (var setting in settings)
            {
                if (!string.IsNullOrWhiteSpace(setting.HardwareId))
                {
                    SendRestoreMonitorStateCommand(setting.HardwareId);
                }
            }

            _isPaused = true;
        }

        public void Resume()
        {
            if (!_isStarted || !_isPaused)
                return;

            Log.Information("ApplicationOrchestrator resumed.");

            var settings = _monitorSettingsFileService.LoadSettings();
            _monitorIdleDetectionService.UpdateSettings(settings);
            _monitorIdleDetectionService.Start();

            _isPaused = false;
        }

        private void SubscribeToEvents()
        {
            _monitorSettingsFileService.SettingsChanged += OnSettingsChanged;
            ApplicationNotifications.RestoreAllMonitorsRequested += RestoreAllMonitors;
        }

        private void UnsubscribeFromEvents()
        {
            _monitorSettingsFileService.SettingsChanged -= OnSettingsChanged;
            ApplicationNotifications.RestoreAllMonitorsRequested -= RestoreAllMonitors;
        }

        private void InitializeStateWatcher()
        {
            _monitorStateWatcher.Start();
        }

        public void RestoreAllMonitors()
        {
            Log.Information("Restoring all monitors brightness levels...");
            SendRestoreBrightnessOnAllMonitorsCommand();
        }

        private void OnSettingsChanged(List<MonitorSettings> settings)
        {
            _monitorIdleDetectionService.UpdateSettings(settings);

            foreach (var setting in settings)
            {
                SendRestoreMonitorStateCommand(setting.HardwareId);
            }
        }

        private void SendRestoreBrightnessOnAllMonitorsCommand()
        {
            var command = new RestoreBrightnessOnAllMonitorsCommand();
            _mediator.SendAsync(command);
            Log.Information("RestoreBrightnessOnAllMonitorsCommand sent.");
        }

        private void SendRestoreMonitorStateCommand(string hardwareId)
        {
            var command = new RestoreMonitorStateCommand { HardwareId = hardwareId };
            _mediator.SendAsync(command);
            Log.Information("RestoreMonitorStateCommand sent for monitor {HardwareId}.", hardwareId);
        }
    }
}