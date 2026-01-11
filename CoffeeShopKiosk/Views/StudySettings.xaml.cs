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
            AmbientEnabled.IsChecked = s.AmbientSoundEnabled;
            SoundChoice.SelectedIndex = 0; // Rain
            Volume.Value = s.AmbientSoundVolume;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            _settingsService.Update(s =>
            {
                s.SpotlightEnabled = Spotlight.IsChecked == true;
                s.HideProductImages = HideImages.IsChecked == true;
                s.AmbientSoundEnabled = AmbientEnabled.IsChecked == true;
                s.AmbientSoundChoice = (SoundChoice.SelectedValue ?? "Rain").ToString();
                s.AmbientSoundVolume = Volume.Value;
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