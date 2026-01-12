using System.Windows;
using CoffeeShopKiosk.Services;

namespace CoffeeShopKiosk.Views
{
    public partial class StudySettings : Window
    {
        private readonly SettingsService _settingsService = new SettingsService();

        public StudySettings()
        {
            InitializeComponent();

            var s = _settingsService.Settings;
            Spotlight.IsChecked = s.SpotlightEnabled;
            HideImages.IsChecked = s.HideProductImages;
            EnableAI.IsChecked = s.EnableAIStudyAssistant;
            ThemeChoice.SelectedValue = s.VisualTheme;
            EnableVisuals.IsChecked = s.VisualEffectsEnabled;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            _settingsService.Update(st =>
            {
                st.SpotlightEnabled = Spotlight.IsChecked == true;
                st.HideProductImages = HideImages.IsChecked == true;
                st.EnableAIStudyAssistant = EnableAI.IsChecked == true;
                st.VisualTheme = (ThemeChoice.SelectedValue ?? "Cafe").ToString();
                st.VisualEffectsEnabled = EnableVisuals.IsChecked == true;
            });

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}