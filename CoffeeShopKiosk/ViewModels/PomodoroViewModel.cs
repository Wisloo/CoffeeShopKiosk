using System;
using System.Windows.Input;
using System.Windows.Threading;
using CoffeeShopKiosk.Services;

namespace CoffeeShopKiosk.ViewModels
{
    public class PomodoroViewModel : BaseViewModel
    {
        private readonly DispatcherTimer _timer;
        private readonly SettingsService _settingsService;
        private TimeSpan _remaining;
        private bool _isRunning;
        private bool _isWorkPeriod = true;

        public event Action<bool> PeriodSwitched;

        public ICommand StartPauseCommand { get; }
        public ICommand ResetCommand { get; }

        public PomodoroViewModel(SettingsService settings)
        {
            _settingsService = settings ?? new SettingsService();
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += (s, e) => Tick();

            StartPauseCommand = new RelayCommand<object>(_ => Toggle());
            ResetCommand = new RelayCommand<object>(_ => Reset());

            Reset();
        }

        private void Tick()
        {
            if (!_isRunning) return;

            if (_remaining.TotalSeconds > 0)
            {
                Remaining = _remaining - TimeSpan.FromSeconds(1);
            }
            else
            {
                // Period ended, switch modes
                _isWorkPeriod = !_isWorkPeriod;
                InitializePeriod();
                // Notify via toast service (UI-level), the MainWindow listens for changes and can show toasts if desired
                PeriodSwitched?.Invoke(_isWorkPeriod);
            }
        }

        private void InitializePeriod()
        {
            var minutes = _isWorkPeriod ? _settingsService.Settings.PomodoroWorkMinutes : _settingsService.Settings.PomodoroBreakMinutes;
            Remaining = TimeSpan.FromMinutes(minutes);
        }

        public void Start()
        {
            if (!_isRunning)
            {
                _isRunning = true;
                _timer.Start();
                OnPropertyChanged(nameof(IsRunning));
            }
        }

        public void Pause()
        {
            if (_isRunning)
            {
                _isRunning = false;
                _timer.Stop();
                OnPropertyChanged(nameof(IsRunning));
            }
        }

        public void Toggle()
        {
            if (IsRunning) Pause(); else Start();
        }

        public void Reset()
        {
            _isWorkPeriod = true;
            InitializePeriod();
            Pause();
        }

        public bool IsRunning => _isRunning;
        public bool IsWorkPeriod => _isWorkPeriod;

        public TimeSpan Remaining
        {
            get => _remaining;
            private set
            {
                _remaining = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplayTime));
            }
        }

        public string DisplayTime => Remaining.ToString(@"mm\:ss");
    }
}