using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace CoffeeShopKiosk.Services
{
    public class Toast
    {
        public string Message { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;
    }

    public class ToastService
    {
        public ObservableCollection<Toast> Toasts { get; } = new ObservableCollection<Toast>();

        public void Show(string message, int durationMs = 2800)
        {
            var t = new Toast { Message = message };
            App.Current.Dispatcher.Invoke(() => Toasts.Add(t));

            _ = DismissAfter(t, durationMs);
        }

        private async Task DismissAfter(Toast t, int ms)
        {
            await Task.Delay(ms);
            App.Current.Dispatcher.Invoke(() => Toasts.Remove(t));
        }
    }
}