namespace OLED_Sleeper.Infrastructure
{
    public class ApplicationOptions
    {
        public bool StartPaused { get; set; }
        public bool StartHidden { get; set; }
        public bool ExitImmediately { get; set; }
        public bool PauseImmediately { get; set; }
        public bool ResumeImmediately { get; set; }
        public bool StartMinimized { get; set; }
        public bool TrayOnly { get; set; }
    }
}