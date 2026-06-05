using System.Linq;
using OLED_Sleeper.Core.Interfaces;
using OLED_Sleeper.Features.MonitorBehavior.Commands;
using OLED_Sleeper.Features.MonitorBlackout.Commands;
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
        private bool _monitor1ForcedBlackout;
        private bool _monitor2ForcedBlackout;

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
            _monitor1ForcedBlackout = false;
            _monitor2ForcedBlackout = false;
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

            if (!_monitor1ForcedBlackout && !_monitor2ForcedBlackout)
            {
                _monitorIdleDetectionService.Start();
            }

            _isPaused = false;
        }

        public void BlackoutMonitor1()
        {
            BlackoutMonitorByIndex(0, monitorNumber: 1, setFlag: () => _monitor1ForcedBlackout = true);
        }

        public void EndBlackoutMonitor1()
        {
            EndBlackoutMonitorByIndex(
                0,
                monitorNumber: 1,
                clearFlag: () => _monitor1ForcedBlackout = false);
        }

        public void BlackoutMonitor2()
        {
            BlackoutMonitorByIndex(1, monitorNumber: 2, setFlag: () => _monitor2ForcedBlackout = true);
        }

        public void EndBlackoutMonitor2()
        {
            EndBlackoutMonitorByIndex(
                1,
                monitorNumber: 2,
                clearFlag: () => _monitor2ForcedBlackout = false);
        }

        private void BlackoutMonitorByIndex(int index, int monitorNumber, Action setFlag)
        {
            if (!_isStarted)
                return;

            var settings = _monitorSettingsFileService.LoadSettings();
            var target = settings.ElementAtOrDefault(index);

            if (target == null || string.IsNullOrWhiteSpace(target.HardwareId))
            {
                Log.Warning("BlackoutMonitor{MonitorNumber}: no monitor settings found.", monitorNumber);
                return;
            }

            Log.Information(
                "BlackoutMonitor{MonitorNumber}: forced blackout on monitor {HardwareId}.",
                monitorNumber,
                target.HardwareId);

            setFlag();

            _monitorIdleDetectionService.Stop();

            var command = new ApplyBlackoutOverlayCommand { HardwareId = target.HardwareId };
            _mediator.SendAsync(command);
        }

        private void EndBlackoutMonitorByIndex(int index, int monitorNumber, Action clearFlag)
        {
            if (!_isStarted)
                return;

            var settings = _monitorSettingsFileService.LoadSettings();
            var target = settings.ElementAtOrDefault(index);

            if (target == null || string.IsNullOrWhiteSpace(target.HardwareId))
            {
                Log.Warning("EndBlackoutMonitor{MonitorNumber}: no monitor settings found.", monitorNumber);
                return;
            }

            Log.Information(
                "EndBlackoutMonitor{MonitorNumber}: removing forced blackout from monitor {HardwareId}.",
                monitorNumber,
                target.HardwareId);

            clearFlag();

            var command = new HideBlackoutOverlayCommand { HardwareId = target.HardwareId };
            _mediator.SendAsync(command);

            if (!_isPaused && !_monitor1ForcedBlackout && !_monitor2ForcedBlackout)
            {
                _monitorIdleDetectionService.UpdateSettings(settings);
                _monitorIdleDetectionService.Start();
            }
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