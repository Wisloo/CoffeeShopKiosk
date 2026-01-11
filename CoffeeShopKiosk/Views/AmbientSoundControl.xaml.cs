using System.Windows;
using System.Windows.Controls;
using CoffeeShopKiosk.Services;

namespace CoffeeShopKiosk.Views
{
    public partial class AmbientSoundControl : UserControl
    {
        public AmbientSoundControl()
        {
            InitializeComponent();
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            AmbientSoundService.Play((string)(SoundChoice.SelectedValue ?? "Rain"), Volume.Value);
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            AmbientSoundService.Stop();
        }
    }
}