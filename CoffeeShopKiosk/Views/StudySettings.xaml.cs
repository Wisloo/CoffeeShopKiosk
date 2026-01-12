using System.Windows;
using CoffeeShopKiosk.Services;

namespace CoffeeShopKiosk.Views
{
    public partial class StudySettings : Window
    {
        private readonly SettingsService _settingsService = new SettingsService();

        private bool _isDirty = false;

        public StudySettings()
        {
            InitializeComponent();

            var s = _settingsService.Settings;
            Spotlight.IsChecked = s.SpotlightEnabled;
            HideImages.IsChecked = s.HideProductImages;
            EnableAI.IsChecked = s.EnableAIStudyAssistant;
            ApiKeyBox.Password = s.OpenAIKey ?? string.Empty;
            EnableDebug.IsChecked = s.EnableAIDebugLogging;
            ThemeChoice.SelectedValue = s.VisualTheme;
            EnableVisuals.IsChecked = s.VisualEffectsEnabled;

            // Track changes to prompt for save on close
            Spotlight.Checked += (_,__) => _isDirty = true; Spotlight.Unchecked += (_,__) => _isDirty = true;
            HideImages.Checked += (_,__) => _isDirty = true; HideImages.Unchecked += (_,__) => _isDirty = true;
            EnableAI.Checked += (_,__) => _isDirty = true; EnableAI.Unchecked += (_,__) => _isDirty = true;
            EnableDebug.Checked += (_,__) => _isDirty = true; EnableDebug.Unchecked += (_,__) => _isDirty = true;
            ApiKeyBox.PasswordChanged += (_,__) => _isDirty = true;
            ThemeChoice.SelectionChanged += (_,__) => _isDirty = true;
            EnableVisuals.Checked += (_,__) => _isDirty = true; EnableVisuals.Unchecked += (_,__) => _isDirty = true;
            this.Closing += StudySettings_Closing;
        }

        private void StudySettings_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!_isDirty) return;
            var res = MessageBox.Show("You have unsaved changes. Do you want to save them?", "Unsaved Changes", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
            if (res == MessageBoxResult.Yes)
            {
                // Save settings without calling Close (window is already closing)
                SaveSettings();
            }
            else if (res == MessageBoxResult.Cancel)
            {
                e.Cancel = true;
            }
            else
            {
                // No => discard changes (do nothing)
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            _settingsService.Update(st =>
            {
                st.SpotlightEnabled = Spotlight.IsChecked == true;
                st.HideProductImages = HideImages.IsChecked == true;
                st.EnableAIStudyAssistant = EnableAI.IsChecked == true;
                st.OpenAIKey = ApiKeyBox.Password ?? string.Empty;
                st.EnableAIDebugLogging = EnableDebug.IsChecked == true;
                st.VisualTheme = (ThemeChoice.SelectedValue ?? "Cafe").ToString();
                st.VisualEffectsEnabled = EnableVisuals.IsChecked == true;
            });

            DialogResult = true;
            Close();
        }

        private async void TestApiKey_Click(object sender, RoutedEventArgs e)
        {
            // Save the key temporarily then test connectivity
            var key = ApiKeyBox.Password ?? string.Empty;
            _settingsService.Update(st => st.OpenAIKey = key);

            var svc = new Services.AIService();
            var ok = await svc.TestApiKeyAsync();
            if (ok)
            {
                MessageBox.Show("API key looks valid — OpenAI reachable.", "API Key Test", MessageBoxButton.OK, MessageBoxImage.Information);
                EnableAI.IsChecked = true;
            }
            else
            {
                MessageBox.Show("API key test failed. Check the key or network connectivity.", "API Key Test", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void SaveSettings()
        {
            _settingsService.Update(st =>
            {
                st.SpotlightEnabled = Spotlight.IsChecked == true;
                st.HideProductImages = HideImages.IsChecked == true;
                st.EnableAIStudyAssistant = EnableAI.IsChecked == true;
                st.OpenAIKey = ApiKeyBox.Password ?? string.Empty;
                st.EnableAIDebugLogging = EnableDebug.IsChecked == true;
                st.VisualTheme = (ThemeChoice.SelectedValue ?? "Cafe").ToString();
                st.VisualEffectsEnabled = EnableVisuals.IsChecked == true;
            });

            DialogResult = true;
        }
    }
}