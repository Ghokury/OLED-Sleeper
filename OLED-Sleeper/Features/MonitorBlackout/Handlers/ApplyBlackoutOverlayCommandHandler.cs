using System.Linq;
using OLED_Sleeper.Core.Interfaces;
using OLED_Sleeper.Features.MonitorBlackout.Commands;
using OLED_Sleeper.Features.MonitorBlackout.Services.Interfaces;
using OLED_Sleeper.Features.MonitorDimming.Services.Interfaces;
using OLED_Sleeper.Features.MonitorInformation.Models;
using OLED_Sleeper.Features.MonitorInformation.Services.Interfaces;
using Serilog;

namespace OLED_Sleeper.Features.MonitorBlackout.Handlers
{
    /// <summary>
    /// Handles the execution of the <see cref="ApplyBlackoutOverlayCommand"/>.
    /// This class contains the business logic for applying the blackout effect to a monitor,
    /// which now uses only a software overlay and does not modify hardware brightness.
    /// </summary>
    public class ApplyBlackoutOverlayCommandHandler : ICommandHandler<ApplyBlackoutOverlayCommand>
    {
        private readonly IMonitorInfoManager _monitorInfoManager;
        private readonly IMonitorBlackoutService _monitorBlackoutService;
        private readonly IMonitorDimmingService _monitorDimmingService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplyBlackoutOverlayCommandHandler"/> class.
        /// </summary>
        /// <param name="monitorInfoManager">Service that provides monitor information.</param>
        /// <param name="monitorBlackoutService">The service responsible for showing/hiding blackout overlays.</param>
        /// <param name="monitorDimmingService">The service responsible for controlling monitor brightness.</param>
        public ApplyBlackoutOverlayCommandHandler(
            IMonitorInfoManager monitorInfoManager,
            IMonitorBlackoutService monitorBlackoutService,
            IMonitorDimmingService monitorDimmingService)
        {
            _monitorInfoManager = monitorInfoManager;
            _monitorBlackoutService = monitorBlackoutService;
            _monitorDimmingService = monitorDimmingService;
        }

        /// <summary>
        /// Executes the blackout logic asynchronously based on the command's data.
        /// It shows a blackout overlay but does not change hardware brightness anymore.
        /// Exceptions are caught and logged to avoid silent failures.
        /// </summary>
        /// <param name="command">The command containing the details of the monitor to black out.</param>
        public async Task HandleAsync(ApplyBlackoutOverlayCommand command)
        {
            try
            {
                Log.Information("Executing ApplyBlackoutCommand for monitor {HardwareId}.", command.HardwareId);

                var monitorInfo = await GetMonitorInfoAsync(command.HardwareId);
                if (monitorInfo == null)
                {
                    Log.Warning("Monitor info not found for HardwareId {HardwareId}.", command.HardwareId);
                    return;
                }

                // Blackout is implemented purely as a software overlay.
                // We intentionally do not touch hardware brightness here
                // to avoid leaving the monitor stuck at minimal brightness
                // if the app is force-closed or crashes.
                await _monitorBlackoutService.ShowBlackoutOverlayAsync(monitorInfo.HardwareId, monitorInfo.Bounds);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to apply blackout for monitor {HardwareId}.", command.HardwareId);
            }
        }

        /// <summary>
        /// Asynchronously retrieves the <see cref="MonitorInfo"/> for the specified hardware ID by awaiting the MonitorListReady event.
        /// This method bridges the event-based monitor info retrieval to an awaitable Task, ensuring the handler can work with up-to-date monitor data.
        /// </summary>
        /// <param name="hardwareId">The unique hardware ID of the monitor to retrieve.</param>
        /// <returns>The <see cref="MonitorInfo"/> for the specified hardware ID, or null if not found.</returns>
        private async Task<MonitorInfo?> GetMonitorInfoAsync(string? hardwareId)
        {
            var tcs = new TaskCompletionSource<MonitorInfo?>();

            void Handler(object? sender, IReadOnlyList<MonitorInfo> monitors)
            {
                _monitorInfoManager.MonitorListReady -= Handler;
                var monitor = monitors.FirstOrDefault(m => m.HardwareId == hardwareId);
                tcs.SetResult(monitor);
            }

            _monitorInfoManager.MonitorListReady += Handler;
            _monitorInfoManager.GetCurrentMonitorsAsync();

            return await tcs.Task;
        }
    }
}