
using System.Windows;
using CoffeeShopKiosk.ViewModels;

namespace CoffeeShopKiosk
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var vm = new MainViewModel();
            DataContext = vm;

            // Show a simple confirmation when an order is placed
            vm.Cart.OrderPlaced += order =>
            {
                MessageBox.Show($"Order {order.OrderId} placed — Total {order.TotalAmount:C}", "Order Complete", MessageBoxButton.OK, MessageBoxImage.Information);
            };
        }
    }
}