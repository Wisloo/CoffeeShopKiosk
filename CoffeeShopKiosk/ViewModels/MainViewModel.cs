using System.Collections.ObjectModel;
using System.Windows.Input;
using CoffeeShopKiosk.Models;
using CoffeeShopKiosk.Services;

namespace CoffeeShopKiosk.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly ProductService _productService;
        private readonly OrderService _orderService;
        private ObservableCollection<ProductModel> _products;

        public ObservableCollection<ProductModel> Products
        {
            get { return _products; }
            set { _products = value; OnPropertyChanged(); }
        }

        public CartViewModel Cart { get; }

        public ICommand AddToOrderCommand { get; }

        public MainViewModel()
        {
            _productService = new ProductService();
            _orderService = new OrderService();
            Cart = new CartViewModel(_orderService);

            LoadProducts();

            AddToOrderCommand = new RelayCommand<ProductModel>(p => Cart.AddToCart(p));
        }

        private void LoadProducts()
        {
            Products = new ObservableCollection<ProductModel>(_productService.GetAllProducts());
        }
    }
}