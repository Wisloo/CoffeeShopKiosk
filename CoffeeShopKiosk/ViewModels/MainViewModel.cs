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
        public ICommand ToggleStudyModeCommand { get; }

        public MainViewModel()
        {
            _productService = new ProductService();
            _orderService = new OrderService();
            _settingsService = new SettingsService();
            Cart = new CartViewModel(_orderService);
            Pomodoro = new PomodoroViewModel(_settingsService);

            LoadProducts();

            AddToOrderCommand = new RelayCommand<ProductModel>(p => Cart.AddToCart(p));
            // Accept an optional parameter (bool) to explicitly set Study Mode; otherwise toggle
            ToggleStudyModeCommand = new RelayCommand<object>(p => ToggleStudyMode(p));
        }

        private void LoadProducts()
        {
            Products = new ObservableCollection<ProductModel>(_productService.GetAllProducts());
        }
        private void ToggleStudyMode(object parameter)
        {
            if (parameter is bool explicitValue)
            {
                _settingsService.Update(s => s.StudyMode = explicitValue);
            }
            else
            {
                var current = _settingsService.Settings.StudyMode;
                _settingsService.Update(s => s.StudyMode = !current);
            }

            // Raise property changed so views can react
            OnPropertyChanged(nameof(IsStudyMode));
        }

        public bool IsStudyMode
        {
            get => _settingsService.Settings.StudyMode;
            set
            {
                _settingsService.Update(s => s.StudyMode = value);
                OnPropertyChanged();
            }
        }

        // Exposed for product list to hide images when Study Mode + setting is enabled
        private bool _isHideProductImages;
        public bool IsHideProductImages
        {
            get => _isHideProductImages;
            set { _isHideProductImages = value; OnPropertyChanged(); }
        }
    }
}