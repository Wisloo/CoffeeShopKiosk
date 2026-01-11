using System.Windows.Controls;

namespace CoffeeShopKiosk.Views
{
    public partial class SpotlightOverlay : UserControl
    {
        public SpotlightOverlay()
        {
            InitializeComponent();
        }

        public void Show(bool visible, double opacity = 0.5)
        {
            Dim.Visibility = visible ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            Dim.Opacity = opacity;
        }
    }
}