using System.Windows;
using CoffeeShopKiosk.Models;

namespace CoffeeShopKiosk.Views
{
    public partial class ReceiptModal : Window
    {
        public ReceiptModal(OrderModel order)
        {
            InitializeComponent();
            DataContext = order;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}