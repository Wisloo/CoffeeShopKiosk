using System.Windows.Controls;
using System.Windows;
using CoffeeShopKiosk.ViewModels;

namespace CoffeeShopKiosk.Views
{
    public partial class CartView : UserControl
    {
        public CartView()
        {
            InitializeComponent();
            // DataContext is set from MainWindow (binding to MainViewModel.Cart)
        }
    }
}