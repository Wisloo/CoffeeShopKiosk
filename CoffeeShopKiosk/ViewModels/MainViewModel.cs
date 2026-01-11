using System.Collections.ObjectModel;
using System.Windows.Input;
using CoffeeShopKiosk.Models;
using CoffeeShopKiosk.Services;

namespace CoffeeShopKiosk.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly ProductService _productService;
        private ObservableCollection<ProductModel> _products;

        public ObservableCollection<ProductModel> Products
        {
            get { return _products; }
            set { _products = value; OnPropertyChanged(); }
        }

        public ICommand PurchaseCommand { get; }

        public MainViewModel()
        {
            _productService = new ProductService();
            LoadProducts();
            PurchaseCommand = new RelayCommand<ProductModel>(PurchaseProduct);
        }

        private void LoadProducts()
        {
            Products = new ObservableCollection<ProductModel>(_productService.GetAllProducts());
        }

        private void PurchaseProduct(ProductModel product)
        {
            // Logic to handle product purchase
        }
    }
}