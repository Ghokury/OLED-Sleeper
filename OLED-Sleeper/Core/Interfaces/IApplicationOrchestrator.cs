namespace OLED_Sleeper.Core.Interfaces
{
    /// <summary>
    /// Defines the contract for the application orchestrator, which coordinates monitor management and application lifecycle.
    /// </summary>
    public interface IApplicationOrchestrator
    {
        /// <summary>
        /// Starts the orchestrator, initializing all monitor management logic and event subscriptions.
        /// </summary>
        void Start();

        /// <summary>
        /// Stops the orchestrator, unsubscribing from events and restoring monitor states.
        /// </summary>
        void Stop();

        /// <summary>
        /// Pauses idle detection and restores monitors to their active state.
        /// </summary>
        void Pause();

        /// <summary>
        /// Resumes idle detection.
        /// </summary>
        void Resume();

        /// <summary>
        /// Forces blackout on monitor 1.
        /// </summary>
        void BlackoutMonitor1();

        /// <summary>
        /// Removes forced blackout from monitor 1.
        /// </summary>
        void EndBlackoutMonitor1();

        /// <summary>
        /// Forces blackout on monitor 2.
        /// </summary>
        void BlackoutMonitor2();

        /// <summary>
        /// Removes forced blackout from monitor 2.
        /// </summary>
        void EndBlackoutMonitor2();

        /// <summary>
        /// Restores brightness on all monitors.
        /// </summary>
        void RestoreAllMonitors();
    }
}