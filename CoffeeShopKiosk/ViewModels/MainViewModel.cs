using System.Collections.ObjectModel;
using System.Windows.Input;
using CoffeeShopKiosk.Models;
using CoffeeShopKiosk.Services;

namespace CoffeeShopKiosk.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly ProductService _productService;
        private readonly OrderService _orderService;        private readonly SettingsService _settingsService;        private ObservableCollection<ProductModel> _products;

        public ObservableCollection<ProductModel> Products
        {
            get { return _products; }
            set { _products = value; OnPropertyChanged(); }
        }

        public CartViewModel Cart { get; }

        public ICommand AddToOrderCommand { get; }
        public ICommand ToggleStudyModeCommand { get; }

        public MainViewModel()
        {
            _productService = new ProductService();
            _orderService = new OrderService();
            _settingsService = new SettingsService();
            Cart = new CartViewModel(_orderService);

            LoadProducts();

            AddToOrderCommand = new RelayCommand<ProductModel>(p => Cart.AddToCart(p));
            ToggleStudyModeCommand = new RelayCommand<object>(_ => ToggleStudyMode());
        }

        private void LoadProducts()
        {
            Products = new ObservableCollection<ProductModel>(_productService.GetAllProducts());
        }
        private void ToggleStudyMode()
        {
            var current = _settingsService.Settings.StudyMode;
            _settingsService.Update(s => s.StudyMode = !current);
            OnPropertyChanged(nameof(IsStudyMode));
        }

        public bool IsStudyMode => _settingsService.Settings.StudyMode;    }
}