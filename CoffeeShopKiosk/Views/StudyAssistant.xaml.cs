using System;
using System.Linq;
using System.Windows;
using CoffeeShopKiosk.Services;
using CoffeeShopKiosk.ViewModels;
using CoffeeShopKiosk.Models;

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

            // If the assistant produced a fallback message, notify the user and suggest enabling debug logging
            var lastAssistant = _vm.Messages.LastOrDefault(m => m.Role == "assistant");
            if (lastAssistant != null && lastAssistant.Source == "fallback")
            {
                MessageBox.Show("The assistant couldn't generate a full answer. Try again or enable AI debug logging in Visual Settings and then use 'Show raw response' to inspect the response.", "Incomplete Answer", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void SourceLink_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is System.Windows.Controls.TextBlock tb && Uri.IsWellFormedUriString(tb.Text, UriKind.Absolute))
            {
                try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(tb.Text) { UseShellExecute = true }); } catch { }
            }
        }

        private void FeedbackYes_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button b && b.Tag is ChatMessage cm)
            {
                _vm.HelpFeedbackCommand.Execute(System.Tuple.Create(cm, true));
            }
        }

        private void FeedbackNo_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button b && b.Tag is ChatMessage cm)
            {
                _vm.HelpFeedbackCommand.Execute(System.Tuple.Create(cm, false));
            }
        }

        private void ShowRaw_Click(object sender, RoutedEventArgs e)
        {
            var settings = new SettingsService();
            if (!settings.Settings.EnableAIDebugLogging)
            {
                MessageBox.Show("AI debug logging is disabled. Enable it in Visual Settings to see raw responses.", "Debug Disabled", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var telemetry = new TelemetryService();
            var last = telemetry.GetLastRawResponse();
            if (string.IsNullOrWhiteSpace(last))
            {
                MessageBox.Show("No raw responses found yet.", "Raw Response", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                // Show a better formatted view that lets the user copy the raw response
                var win = new Window { Title = "Raw Response", Width = 600, Height = 400, Owner = this };
                var tb = new System.Windows.Controls.TextBox { Text = last, IsReadOnly = true, TextWrapping = System.Windows.TextWrapping.Wrap, VerticalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Auto };
                win.Content = tb;
                win.ShowDialog();
            }
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