using System.Windows;
using CoffeeShopKiosk.Models;

namespace CoffeeShopKiosk.Views
{
    public partial class ReceiptModal : Window
    {
        // Parameterless ctor for designer and IntelliSense to instantiate at design-time
        public ReceiptModal()
        {
            InitializeComponent();
        }

        public ReceiptModal(OrderModel order) : this()
        {
            DataContext = order;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}