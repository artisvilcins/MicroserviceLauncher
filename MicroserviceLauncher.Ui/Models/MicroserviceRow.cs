namespace MicroserviceLauncher.Uiw
{
    public class MicroserviceRow : MicroserviceConfig
    {
        public delegate void IsRunningChangeHandler(bool isRunning);
        public event IsRunningChangeHandler IsRunningChange;

        private bool isRunning;
        public bool IsRunning
        {
            get
            {
                return isRunning;
            }
            set
            {
                IsRunningChange?.Invoke(value);
                isRunning = value;
            }
        }
    }
}