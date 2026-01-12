using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using CoffeeShopKiosk.Models;
using CoffeeShopKiosk.Services;

namespace CoffeeShopKiosk.ViewModels
{
    public class StudyChatViewModel : INotifyPropertyChanged
    {
        private readonly IAIService _ai;
        private string _inputText = string.Empty;
        private bool _isBusy;

        public StudyChatViewModel(IAIService ai)
        {
            _ai = ai;
            Messages = new ObservableCollection<ChatMessage>();
            SendCommand = new RelayCommand<object>(async _ => await Send(), _ => !IsBusy);
            ClearCommand = new RelayCommand<object>(_ => Messages.Clear(), _ => !IsBusy);
        }

        public ObservableCollection<ChatMessage> Messages { get; }

        public string InputText
        {
            get => _inputText;
            set { _inputText = value; OnPropertyChanged(); }
        }

        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(); }
        }

        public ICommand SendCommand { get; }
        public ICommand ClearCommand { get; }

        public async Task Send()
        {
            var text = InputText?.Trim();
            if (string.IsNullOrEmpty(text)) return;

            try
            {
                IsBusy = true;
                // append user message
                Messages.Add(new ChatMessage { Role = "user", Text = text, Timestamp = DateTimeOffset.Now });
                InputText = string.Empty;

                var reply = await _ai.ChatAsync(text);
                Messages.Add(new ChatMessage { Role = "assistant", Text = reply ?? "", Timestamp = DateTimeOffset.Now });
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