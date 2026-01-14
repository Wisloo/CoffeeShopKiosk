using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CoffeeShopKiosk.Models
{
    public class ChatMessage : INotifyPropertyChanged
    {
        private string _role = "user";
        private string _text = string.Empty;
        private DateTimeOffset _timestamp = DateTimeOffset.UtcNow;
        private string _source = "local";
        private double _confidence = 0.0;
        private ObservableCollection<string> _sources = new ObservableCollection<string>();

        public string Role { get => _role; set { _role = value; OnPropertyChanged(); } } // "user" or "assistant"
        public string Text { get => _text; set { _text = value; OnPropertyChanged(); } }
        public DateTimeOffset Timestamp { get => _timestamp; set { _timestamp = value; OnPropertyChanged(); } }

        // Optional metadata for display and telemetry
        public string Source { get => _source; set { _source = value; OnPropertyChanged(); } } // e.g., "local" or "remote"
        public double Confidence { get => _confidence; set { _confidence = value; OnPropertyChanged(); } }

        // Optional list of source URLs cited by the assistant
        public ObservableCollection<string> Sources { get => _sources; set { _sources = value ?? new ObservableCollection<string>(); OnPropertyChanged(); } }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
} 