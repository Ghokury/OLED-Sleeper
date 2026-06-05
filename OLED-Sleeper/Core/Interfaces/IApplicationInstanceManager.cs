namespace OLED_Sleeper.Core.Interfaces
{
    public interface IApplicationInstanceManager : IDisposable
    {
        bool IsFirstInstance { get; }

        void Initialize(
            bool requestPause = false,
            bool requestResume = false,
            bool requestExit = false,
            bool requestBlackoutMonitor1 = false,
            bool requestEndBlackoutMonitor1 = false,
            bool requestBlackoutMonitor2 = false,
            bool requestEndBlackoutMonitor2 = false);

        void SetShowMainWindowAction(Action showMainWindowAction);
        void SetPauseAction(Action pauseAction);
        void SetResumeAction(Action resumeAction);
        void SetExitAction(Action exitAction);

        void SetBlackoutMonitor1Action(Action blackoutMonitor1Action);
        void SetEndBlackoutMonitor1Action(Action endBlackoutMonitor1Action);
        void SetBlackoutMonitor2Action(Action blackoutMonitor2Action);
        void SetEndBlackoutMonitor2Action(Action endBlackoutMonitor2Action);
    }
}