namespace OLED_Sleeper.Core.Interfaces
{
    /// <summary>
    /// Defines the contract for managing single-instance enforcement and inter-process signaling.
    /// </summary>
    public interface IApplicationInstanceManager : IDisposable
    {
        /// <summary>
        /// Indicates whether this is the first instance of the application.
        /// </summary>
        bool IsFirstInstance { get; }

        /// <summary>
        /// Initializes the single-instance check and event signaling.
        /// </summary>
        void Initialize(
            bool requestPause = false,
            bool requestResume = false,
            bool requestExit = false);

        void SetShowMainWindowAction(Action showMainWindowAction);
        void SetPauseAction(Action pauseAction);
        void SetResumeAction(Action resumeAction);
        void SetExitAction(Action exitAction);
    }
}