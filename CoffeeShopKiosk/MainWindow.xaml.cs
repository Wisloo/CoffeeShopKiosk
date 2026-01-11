
using System.Windows;
using CoffeeShopKiosk.ViewModels;

namespace CoffeeShopKiosk
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }
    }
}