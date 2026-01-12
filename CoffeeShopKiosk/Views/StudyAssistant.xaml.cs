using System.Windows;
using CoffeeShopKiosk.Services;
using CoffeeShopKiosk.ViewModels;

namespace CoffeeShopKiosk.Views
{
    public partial class StudyAssistant : Window
    {
        private readonly StudyChatViewModel _vm;

        public StudyAssistant()
        {
            var settings = new SettingsService();
            if (!settings.Settings.EnableAIStudyAssistant)
            {
                System.Windows.MessageBox.Show("AI Study Assistant is disabled. Enable it in Visual Settings to use this feature.", "AI Disabled", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                Close();
                return;
            }

            InitializeComponent();
            var svc = new AIService();
            _vm = new StudyChatViewModel(svc);
            DataContext = _vm;

            InputBox.SetBinding(System.Windows.Controls.TextBox.TextProperty, new System.Windows.Data.Binding("InputText") { Source = _vm, Mode = System.Windows.Data.BindingMode.TwoWay });
            MessagesList.SetBinding(System.Windows.Controls.ItemsControl.ItemsSourceProperty, new System.Windows.Data.Binding("Messages") { Source = _vm });
            InputBox.SetBinding(System.Windows.Controls.TextBox.TextProperty, new System.Windows.Data.Binding("InputText") { Source = _vm, Mode = System.Windows.Data.BindingMode.TwoWay });
            MessagesList.SetBinding(System.Windows.Controls.ItemsControl.ItemsSourceProperty, new System.Windows.Data.Binding("Messages") { Source = _vm });
        }

        private async void Send_Click(object sender, RoutedEventArgs e)
        {
            await _vm.Send();
            if (MessagesList.Parent is System.Windows.Controls.ScrollViewer sv) sv.ScrollToEnd();
            InputBox.Focus();
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            _vm.ClearCommand.Execute(null);
            InputBox.Focus();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Ensure the input box is focused when the assistant opens so typing works immediately
            InputBox.Focus();
        }

        private async void InputBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // Press Enter (without Shift) to send the message; Shift+Enter allows newlines if AcceptsReturn were enabled
            if (e.Key == System.Windows.Input.Key.Enter && (System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Shift) == 0)
            {
                e.Handled = true;
                await _vm.Send();
                if (MessagesList.Parent is System.Windows.Controls.ScrollViewer sv) sv.ScrollToEnd();
                InputBox.Focus();
            }
        }
    }
}