using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using CoffeeShopKiosk.Services;

namespace CoffeeShopKiosk.ViewModels
{
    public class StudyAssistantViewModel : INotifyPropertyChanged
    {
        private readonly IAIService _ai;
        private string _topic = string.Empty;
        private string _tip = string.Empty;
        private bool _isBusy;

        public StudyAssistantViewModel(IAIService ai)
        {
            _ai = ai;
            GetTipCommand = new RelayCommand<object>(_ => _ = GetTip(), _ => !IsBusy);
        }

        public string Topic
        {
            get => _topic;
            set { _topic = value; OnPropertyChanged(); }
        }

        public string Tip
        {
            get => _tip;
            set { _tip = value; OnPropertyChanged(); }
        }

        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(); }
        }

        public ICommand GetTipCommand { get; }

        public async Task GetTip()
        {
            try
            {
                IsBusy = true;
                Tip = await _ai.GetStudyTipAsync(Topic);
            }
            finally
            {
                IsBusy = false;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}