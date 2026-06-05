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

        private Mutex? _mutex;
        private EventWaitHandle? _showWindowEvent;
        private EventWaitHandle? _pauseEvent;
        private EventWaitHandle? _resumeEvent;
        private EventWaitHandle? _exitEvent;

        private Action? _showMainWindowAction;
        private Action? _pauseAction;
        private Action? _resumeAction;
        private Action? _exitAction;

        public bool IsFirstInstance { get; private set; }

        public void Initialize(bool requestPause = false, bool requestResume = false, bool requestExit = false)
        {
            _mutex = new Mutex(true, MutexName, out bool isNewInstance);
            IsFirstInstance = isNewInstance;

            if (!IsFirstInstance)
            {
                SignalFirstInstanceAndExit(requestPause, requestResume, requestExit);
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

        private void SignalFirstInstanceAndExit(bool requestPause, bool requestResume, bool requestExit)
        {
            try
            {
                if (requestExit)
                {
                    using var exitEvent = EventWaitHandle.OpenExisting(ExitEventName);
                    exitEvent.Set();
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

            Task.Run(ListenLoop);
        }

        private void ListenLoop()
        {
            var handles = new WaitHandle[]
            {
                _showWindowEvent!,
                _pauseEvent!,
                _resumeEvent!,
                _exitEvent!
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