namespace MicroserviceLauncher.Ui.Models
{
    public class MicroserviceRow : MicroserviceConfig
    {
        public delegate void IsRunningChangeHandler(bool isRunning);
        public event IsRunningChangeHandler? IsRunningChange;

        private bool _isRunning;
        public bool IsRunning
        {
            get => _isRunning;
            set
            {
                IsRunningChange?.Invoke(value);
                _isRunning = value;
            }
        }
    }
}