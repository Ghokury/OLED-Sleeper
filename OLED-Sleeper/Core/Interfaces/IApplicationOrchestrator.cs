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
    }
}