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
        public PomodoroViewModel Pomodoro { get; }

        public ICommand AddToOrderCommand { get; }

        // Expose the currently selected visual theme (backed by settings)
        public string VisualTheme
        {
            get => _settingsService.Settings.VisualTheme;
            set
            {
                _settingsService.Update(s => s.VisualTheme = value);
                OnPropertyChanged();
            }
        }

        public MainViewModel()
        {
            _productService = new ProductService();
            _orderService = new OrderService();
            _settingsService = new SettingsService();
            Cart = new CartViewModel(_orderService);
            Pomodoro = new PomodoroViewModel(_settingsService);

            LoadProducts();

            AddToOrderCommand = new RelayCommand<ProductModel>(p => Cart.AddToCart(p));
        }

        private void LoadProducts()
        {
            Products = new ObservableCollection<ProductModel>(_productService.GetAllProducts());
        }




        private bool _isHideProductImages;
        public bool IsHideProductImages
        {
            get => _isHideProductImages;
            set { _isHideProductImages = value; OnPropertyChanged(); }
        }
    }
}