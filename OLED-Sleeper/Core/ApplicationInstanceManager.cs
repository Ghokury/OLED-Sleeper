using OLED_Sleeper.Core.Interfaces;
using System.Windows;

namespace OLED_Sleeper.Core
{
    /// <summary>
    /// Enforces a single running instance of the application and supports IPC commands.
    /// </summary>
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

        private Mutex? _mutex;
        private EventWaitHandle? _showWindowEvent;
        private EventWaitHandle? _pauseEvent;
        private EventWaitHandle? _resumeEvent;
        private EventWaitHandle? _exitEvent;
        private EventWaitHandle? _blackoutMonitor1Event;
        private EventWaitHandle? _endBlackoutMonitor1Event;
        private EventWaitHandle? _blackoutMonitor2Event;
        private EventWaitHandle? _endBlackoutMonitor2Event;

        private Action? _showMainWindowAction;
        private Action? _pauseAction;
        private Action? _resumeAction;
        private Action? _exitAction;
        private Action? _blackoutMonitor1Action;
        private Action? _endBlackoutMonitor1Action;
        private Action? _blackoutMonitor2Action;
        private Action? _endBlackoutMonitor2Action;

        public bool IsFirstInstance { get; private set; }

        public void Initialize(
            bool requestPause = false,
            bool requestResume = false,
            bool requestExit = false,
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
                    requestBlackoutMonitor1,
                    requestEndBlackoutMonitor1,
                    requestBlackoutMonitor2,
                    requestEndBlackoutMonitor2);
                return;
            }

            CreateEventsAndListen();
        }

        public void SetShowMainWindowAction(Action showMainWindowAction)
        {
            _showMainWindowAction = showMainWindowAction ?? throw new ArgumentNullException(nameof(showMainWindowAction));
        }

        public void SetPauseAction(Action pauseAction)
        {
            _pauseAction = pauseAction ?? throw new ArgumentNullException(nameof(pauseAction));
        }

        public void SetResumeAction(Action resumeAction)
        {
            _resumeAction = resumeAction ?? throw new ArgumentNullException(nameof(resumeAction));
        }

        public void SetExitAction(Action exitAction)
        {
            _exitAction = exitAction ?? throw new ArgumentNullException(nameof(exitAction));
        }

        public void SetBlackoutMonitor1Action(Action blackoutMonitor1Action)
        {
            _blackoutMonitor1Action = blackoutMonitor1Action ?? throw new ArgumentNullException(nameof(blackoutMonitor1Action));
        }

        public void SetEndBlackoutMonitor1Action(Action endBlackoutMonitor1Action)
        {
            _endBlackoutMonitor1Action = endBlackoutMonitor1Action ?? throw new ArgumentNullException(nameof(endBlackoutMonitor1Action));
        }

        public void SetBlackoutMonitor2Action(Action blackoutMonitor2Action)
        {
            _blackoutMonitor2Action = blackoutMonitor2Action ?? throw new ArgumentNullException(nameof(blackoutMonitor2Action));
        }

        public void SetEndBlackoutMonitor2Action(Action endBlackoutMonitor2Action)
        {
            _endBlackoutMonitor2Action = endBlackoutMonitor2Action ?? throw new ArgumentNullException(nameof(endBlackoutMonitor2Action));
        }

        private void SignalFirstInstanceAndExit(
            bool requestPause,
            bool requestResume,
            bool requestExit,
            bool requestBlackoutMonitor1,
            bool requestEndBlackoutMonitor1,
            bool requestBlackoutMonitor2,
            bool requestEndBlackoutMonitor2)
        {
            try
            {
                if (requestExit)
                {
                    using var exitEvent = EventWaitHandle.OpenExisting(ExitEventName);
                    exitEvent.Set();
                }
                else if (requestEndBlackoutMonitor2)
                {
                    using var endBlackoutMonitor2Event = EventWaitHandle.OpenExisting(EndBlackoutMonitor2EventName);
                    endBlackoutMonitor2Event.Set();
                }
                else if (requestBlackoutMonitor2)
                {
                    using var blackoutMonitor2Event = EventWaitHandle.OpenExisting(BlackoutMonitor2EventName);
                    blackoutMonitor2Event.Set();
                }
                else if (requestEndBlackoutMonitor1)
                {
                    using var endBlackoutMonitor1Event = EventWaitHandle.OpenExisting(EndBlackoutMonitor1EventName);
                    endBlackoutMonitor1Event.Set();
                }
                else if (requestBlackoutMonitor1)
                {
                    using var blackoutMonitor1Event = EventWaitHandle.OpenExisting(BlackoutMonitor1EventName);
                    blackoutMonitor1Event.Set();
                }
                else if (requestResume)
                {
                    using var resumeEvent = EventWaitHandle.OpenExisting(ResumeEventName);
                    resumeEvent.Set();
                }
                else if (requestPause)
                {
                    using var pauseEvent = EventWaitHandle.OpenExisting(PauseEventName);
                    pauseEvent.Set();
                }
                else
                {
                    using var showWindowEvent = EventWaitHandle.OpenExisting(ShowEventName);
                    showWindowEvent.Set();
                }
            }
            catch
            {
            }

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
                _endBlackoutMonitor2Event!
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
                            case 0:
                                _showMainWindowAction?.Invoke();
                                break;
                            case 1:
                                _pauseAction?.Invoke();
                                break;
                            case 2:
                                _resumeAction?.Invoke();
                                break;
                            case 3:
                                _exitAction?.Invoke();
                                break;
                            case 4:
                                _blackoutMonitor1Action?.Invoke();
                                break;
                            case 5:
                                _endBlackoutMonitor1Action?.Invoke();
                                break;
                            case 6:
                                _blackoutMonitor2Action?.Invoke();
                                break;
                            case 7:
                                _endBlackoutMonitor2Action?.Invoke();
                                break;
                        }
                    });
                }
                catch
                {
                    break;
                }
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

            if (_mutex != null)
            {
                try
                {
                    _mutex.ReleaseMutex();
                }
                catch
                {
                }

                _mutex.Dispose();
            }
        }
    }
}