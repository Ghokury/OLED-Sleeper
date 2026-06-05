using OLED_Sleeper.Core.Interfaces;
using System.Windows;

namespace OLED_Sleeper.Core
{
    public class ApplicationInstanceManager : IApplicationInstanceManager
    {
        private const string MutexName = "OLED-Sleeper-Mutex";
        private const string ShowEventName = "OLED-Sleeper-ShowWindow";
        private const string PauseEventName = "OLED-Sleeper-Pause";
        private const string ResumeEventName = "OLED-Sleeper-Resume";
        private const string ExitEventName = "OLED-Sleeper-Exit";
        private const string BlackoutMonitor1EventName = "OLED-Sleeper-BlackoutMonitor1";
        private const string EndBlackoutMonitor1EventName = "OLED-Sleeper-EndBlackoutMonitor1";
        private const string BlackoutMonitor2EventName = "OLED-Sleeper-BlackoutMonitor2";
        private const string EndBlackoutMonitor2EventName = "OLED-Sleeper-EndBlackoutMonitor2";
        private const string GameModeEventName = "OLED-Sleeper-GameMode";
        private const string WorkModeEventName = "OLED-Sleeper-WorkMode";

        private Mutex? _mutex;
        private EventWaitHandle? _showWindowEvent;
        private EventWaitHandle? _pauseEvent;
        private EventWaitHandle? _resumeEvent;
        private EventWaitHandle? _exitEvent;
        private EventWaitHandle? _blackoutMonitor1Event;
        private EventWaitHandle? _endBlackoutMonitor1Event;
        private EventWaitHandle? _blackoutMonitor2Event;
        private EventWaitHandle? _endBlackoutMonitor2Event;
        private EventWaitHandle? _gameModeEvent;
        private EventWaitHandle? _workModeEvent;

        private Action? _showMainWindowAction;
        private Action? _pauseAction;
        private Action? _resumeAction;
        private Action? _exitAction;
        private Action? _blackoutMonitor1Action;
        private Action? _endBlackoutMonitor1Action;
        private Action? _blackoutMonitor2Action;
        private Action? _endBlackoutMonitor2Action;
        private Action? _gameModeAction;
        private Action? _workModeAction;

        public bool IsFirstInstance { get; private set; }

        public void Initialize(
            bool requestPause = false,
            bool requestResume = false,
            bool requestExit = false,
            bool requestGameMode = false,
            bool requestWorkMode = false,
            bool requestBlackoutMonitor1 = false,
            bool requestEndBlackoutMonitor1 = false,
            bool requestBlackoutMonitor2 = false,
            bool requestEndBlackoutMonitor2 = false)
        {
            _mutex = new Mutex(true, MutexName, out bool isNewInstance);
            IsFirstInstance = isNewInstance;

            if (!IsFirstInstance)
            {
                SignalFirstInstanceAndExit(
                    requestPause,
                    requestResume,
                    requestExit,
                    requestGameMode,
                    requestWorkMode,
                    requestBlackoutMonitor1,
                    requestEndBlackoutMonitor1,
                    requestBlackoutMonitor2,
                    requestEndBlackoutMonitor2);
                return;
            }

            CreateEventsAndListen();
        }

        public void SetShowMainWindowAction(Action showMainWindowAction) => _showMainWindowAction = showMainWindowAction ?? throw new ArgumentNullException(nameof(showMainWindowAction));
        public void SetPauseAction(Action pauseAction) => _pauseAction = pauseAction ?? throw new ArgumentNullException(nameof(pauseAction));
        public void SetResumeAction(Action resumeAction) => _resumeAction = resumeAction ?? throw new ArgumentNullException(nameof(resumeAction));
        public void SetExitAction(Action exitAction) => _exitAction = exitAction ?? throw new ArgumentNullException(nameof(exitAction));
        public void SetBlackoutMonitor1Action(Action a) => _blackoutMonitor1Action = a ?? throw new ArgumentNullException(nameof(a));
        public void SetEndBlackoutMonitor1Action(Action a) => _endBlackoutMonitor1Action = a ?? throw new ArgumentNullException(nameof(a));
        public void SetBlackoutMonitor2Action(Action a) => _blackoutMonitor2Action = a ?? throw new ArgumentNullException(nameof(a));
        public void SetEndBlackoutMonitor2Action(Action a) => _endBlackoutMonitor2Action = a ?? throw new ArgumentNullException(nameof(a));
        public void SetGameModeAction(Action a) => _gameModeAction = a ?? throw new ArgumentNullException(nameof(a));
        public void SetWorkModeAction(Action a) => _workModeAction = a ?? throw new ArgumentNullException(nameof(a));

        private void SignalFirstInstanceAndExit(
            bool requestPause,
            bool requestResume,
            bool requestExit,
            bool requestGameMode,
            bool requestWorkMode,
            bool requestBlackoutMonitor1,
            bool requestEndBlackoutMonitor1,
            bool requestBlackoutMonitor2,
            bool requestEndBlackoutMonitor2)
        {
            try
            {
                if (requestExit)
                {
                    using var e = EventWaitHandle.OpenExisting(ExitEventName);
                    e.Set();
                }
                else if (requestGameMode)
                {
                    using var e = EventWaitHandle.OpenExisting(GameModeEventName);
                    e.Set();
                }
                else if (requestWorkMode)
                {
                    using var e = EventWaitHandle.OpenExisting(WorkModeEventName);
                    e.Set();
                }
                else if (requestEndBlackoutMonitor2)
                {
                    using var e = EventWaitHandle.OpenExisting(EndBlackoutMonitor2EventName);
                    e.Set();
                }
                else if (requestBlackoutMonitor2)
                {
                    using var e = EventWaitHandle.OpenExisting(BlackoutMonitor2EventName);
                    e.Set();
                }
                else if (requestEndBlackoutMonitor1)
                {
                    using var e = EventWaitHandle.OpenExisting(EndBlackoutMonitor1EventName);
                    e.Set();
                }
                else if (requestBlackoutMonitor1)
                {
                    using var e = EventWaitHandle.OpenExisting(BlackoutMonitor1EventName);
                    e.Set();
                }
                else if (requestResume)
                {
                    using var e = EventWaitHandle.OpenExisting(ResumeEventName);
                    e.Set();
                }
                else if (requestPause)
                {
                    using var e = EventWaitHandle.OpenExisting(PauseEventName);
                    e.Set();
                }
                else
                {
                    using var e = EventWaitHandle.OpenExisting(ShowEventName);
                    e.Set();
                }
            }
            catch { }

            Application.Current.Shutdown();
        }

        private void CreateEventsAndListen()
        {
            _showWindowEvent = new EventWaitHandle(false, EventResetMode.AutoReset, ShowEventName);
            _pauseEvent = new EventWaitHandle(false, EventResetMode.AutoReset, PauseEventName);
            _resumeEvent = new EventWaitHandle(false, EventResetMode.AutoReset, ResumeEventName);
            _exitEvent = new EventWaitHandle(false, EventResetMode.AutoReset, ExitEventName);
            _blackoutMonitor1Event = new EventWaitHandle(false, EventResetMode.AutoReset, BlackoutMonitor1EventName);
            _endBlackoutMonitor1Event = new EventWaitHandle(false, EventResetMode.AutoReset, EndBlackoutMonitor1EventName);
            _blackoutMonitor2Event = new EventWaitHandle(false, EventResetMode.AutoReset, BlackoutMonitor2EventName);
            _endBlackoutMonitor2Event = new EventWaitHandle(false, EventResetMode.AutoReset, EndBlackoutMonitor2EventName);
            _gameModeEvent = new EventWaitHandle(false, EventResetMode.AutoReset, GameModeEventName);
            _workModeEvent = new EventWaitHandle(false, EventResetMode.AutoReset, WorkModeEventName);

            Task.Run(ListenLoop);
        }

        private void ListenLoop()
        {
            var handles = new WaitHandle[]
            {
                _showWindowEvent!,
                _pauseEvent!,
                _resumeEvent!,
                _exitEvent!,
                _blackoutMonitor1Event!,
                _endBlackoutMonitor1Event!,
                _blackoutMonitor2Event!,
                _endBlackoutMonitor2Event!,
                _gameModeEvent!,
                _workModeEvent!,
            };

            while (true)
            {
                try
                {
                    int signaled = WaitHandle.WaitAny(handles);
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        switch (signaled)
                        {
                            case 0: _showMainWindowAction?.Invoke(); break;
                            case 1: _pauseAction?.Invoke(); break;
                            case 2: _resumeAction?.Invoke(); break;
                            case 3: _exitAction?.Invoke(); break;
                            case 4: _blackoutMonitor1Action?.Invoke(); break;
                            case 5: _endBlackoutMonitor1Action?.Invoke(); break;
                            case 6: _blackoutMonitor2Action?.Invoke(); break;
                            case 7: _endBlackoutMonitor2Action?.Invoke(); break;
                            case 8: _gameModeAction?.Invoke(); break;
                            case 9: _workModeAction?.Invoke(); break;
                        }
                    });
                }
                catch { break; }
            }
        }

        public void Dispose()
        {
            _showWindowEvent?.Dispose();
            _pauseEvent?.Dispose();
            _resumeEvent?.Dispose();
            _exitEvent?.Dispose();
            _blackoutMonitor1Event?.Dispose();
            _endBlackoutMonitor1Event?.Dispose();
            _blackoutMonitor2Event?.Dispose();
            _endBlackoutMonitor2Event?.Dispose();
            _gameModeEvent?.Dispose();
            _workModeEvent?.Dispose();

            if (_mutex != null)
            {
                try { _mutex.ReleaseMutex(); } catch { }
                _mutex.Dispose();
            }
        }
    }
}