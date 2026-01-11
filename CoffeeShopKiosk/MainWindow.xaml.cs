
using System.Windows;
using CoffeeShopKiosk.ViewModels;
using System.Windows.Media;
using CoffeeShopKiosk.Services;

namespace CoffeeShopKiosk
{
    public partial class MainWindow : Window
    {
        private Services.ToastService _toastService;

        public MainWindow()
        {
            InitializeComponent();
            var vm = new MainViewModel();
            DataContext = vm;

            // Wire up toast service to UI
            _toastService = new ToastService();
            ToastHost.DataContext = _toastService;

            // Show a toast when an item is added to cart
            vm.Cart.ItemAdded += product =>
            {
                _toastService.Show($"Added {product.Name} to cart");
            };

            // Show receipt modal and a toast when an order is placed
            vm.Cart.OrderPlaced += order =>
            {
                _toastService.Show($"Order {order.OrderId} placed — Total {order.TotalAmount:C}");
                var modal = new Views.ReceiptModal(order) { Owner = this };
                modal.ShowDialog();
            };

            // Toggle cart visibility
            ToggleCart.Click += (s, e) =>
            {
                if (CartPanel.Visibility == System.Windows.Visibility.Visible)
                {
                    CartPanel.Visibility = System.Windows.Visibility.Collapsed;
                    ToggleCart.Content = "Show Cart";
                }
                else
                {
                    CartPanel.Visibility = System.Windows.Visibility.Visible;
                    ToggleCart.Content = "Hide Cart";
                }
            };

            // Theme toggle - simple inversion between two palettes
            ThemeToggle.Checked += (s, e) => ApplyStudyTheme(true);
            ThemeToggle.Unchecked += (s, e) => ApplyStudyTheme(false);
        }

        private void ApplyStudyTheme(bool study)
        {
            if (study)
            {
                Resources["CoffeeBrownColor"] = (Color)ColorConverter.ConvertFromString("#2B1A12");
                Resources["CreamColor"] = (Color)ColorConverter.ConvertFromString("#F5EFE9");
                Resources["CardColor"] = (Color)ColorConverter.ConvertFromString("#F6EEE8");
            }
            else
            {
                Resources["CoffeeBrownColor"] = (Color)ColorConverter.ConvertFromString("#4B2E19");
                Resources["CreamColor"] = (Color)ColorConverter.ConvertFromString("#FFF8F0");
                Resources["CardColor"] = (Color)ColorConverter.ConvertFromString("#FFF6EE");
            }
        }
    }
}