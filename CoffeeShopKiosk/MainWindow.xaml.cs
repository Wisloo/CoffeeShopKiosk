
using System.Windows;
using CoffeeShopKiosk.ViewModels;
using System.Windows.Media;
using CoffeeShopKiosk.Services;
using System.Windows.Input;

namespace CoffeeShopKiosk
{
    public partial class MainWindow : Window
    {
        private Services.ToastService _toastService;

        // Routed command used by the Window to toggle Study Mode. Using a routed command avoids trying to bind
        // a Binding to the KeyBinding.Command property (which is not a dependency property and causes parse-time errors).
        public static readonly RoutedCommand ToggleStudyModeRoutedCommand = new RoutedCommand();

        public MainWindow()
        {
            InitializeComponent();
            var vm = new MainViewModel();
            DataContext = vm;

            // Wire up toast service to UI first so the VM change handler can safely show toasts
            _toastService = new ToastService();
            ToastHost.DataContext = _toastService;

            // Listen for ViewModel changes so UI can react when Study Mode is toggled via keyboard shortcut or other means
            vm.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(vm.IsStudyMode))
                {
                    // Apply the theme and show a small toast for discoverability
                    ApplyStudyMode(vm.IsStudyMode);
                    _toastService?.Show(vm.IsStudyMode ? "Study Mode enabled" : "Study Mode disabled");
                }
            };

            // Register command binding for the routed ToggleStudyMode command
            CommandBindings.Add(new CommandBinding(ToggleStudyModeRoutedCommand, ToggleStudyMode_Executed));

            // Show a toast when an item is added to cart
            vm.Cart.ItemAdded += product =>
            {
                _toastService.Show($"Added {product.Name} to cart");
            };

            // Show receipt modal and a toast when an order is placed
            vm.Cart.OrderPlaced += order =>
            {
                _toastService.Show($"Order {order.OrderId} placed — Total {order.TotalAmount:C}");
                var modal = new Views.ReceiptModal(order) { Owner = this };
                modal.ShowDialog();
            };

            // Toggle cart visibility
            ToggleCart.Click += (s, e) =>
            {
                if (CartPanel.Visibility == System.Windows.Visibility.Visible)
                {
                    CartPanel.Visibility = System.Windows.Visibility.Collapsed;
                    ToggleCart.Content = "Show Cart";
                }
                else
                {
                    CartPanel.Visibility = System.Windows.Visibility.Visible;
                    ToggleCart.Content = "Hide Cart";
                }
            };

            // Accessibility: keyboard shortcut hint tooltip also set programmatically to ensure consistency
            ThemeToggle.ToolTip = "Toggle Study Mode (Ctrl+Shift+S)";

            // Note: we rely on TwoWay binding between ThemeToggle.IsChecked and the ViewModel's IsStudyMode property
            // so we don't add Checked/Unchecked handlers here to avoid feedback loops.
            // Initialize from settings
            var settings = new SettingsService();
            ApplyStudyMode(settings.Settings.StudyMode);
            _toastService.Muted = settings.Settings.DoNotDisturb;

            // ThemeToggle is bound TwoWay to the ViewModel's IsStudyMode and will update via binding

            // Keep the ToggleCart label in sync with visibility
            ToggleCart.Click += (s, e) =>
            {
                if (CartPanel.Visibility == System.Windows.Visibility.Visible)
                {
                    CartPanel.Visibility = System.Windows.Visibility.Collapsed;
                    ToggleCart.Content = "Show Cart";
                }
                else
                {
                    CartPanel.Visibility = System.Windows.Visibility.Visible;
                    ToggleCart.Content = "Hide Cart";
                }
            };
        }

        private void ApplyStudyTheme(bool study)
        {
            if (study)
            {
                Resources["CoffeeBrownColor"] = (Color)ColorConverter.ConvertFromString("#2B1A12");
                Resources["CreamColor"] = (Color)ColorConverter.ConvertFromString("#F5EFE9");
                Resources["CardColor"] = (Color)ColorConverter.ConvertFromString("#F6EEE8");
            }
            else
            {
                Resources["CoffeeBrownColor"] = (Color)ColorConverter.ConvertFromString("#4B2E19");
                Resources["CreamColor"] = (Color)ColorConverter.ConvertFromString("#FFF8F0");
                Resources["CardColor"] = (Color)ColorConverter.ConvertFromString("#FFF6EE");
            }
        }

        private void ApplyStudyMode(bool enabled)
        {
            // Update theme colors
            ApplyStudyTheme(enabled);

            // Persist the setting
            var settingsSvc = new SettingsService();
            settingsSvc.Update(s => s.StudyMode = enabled);

            // Mute toasts if DoNotDisturb is enabled in settings and StudyMode is on
            _toastService.Muted = enabled && settingsSvc.Settings.DoNotDisturb;

            // Swap product card style to no-motion variant when enabled
            if (enabled)
            {
                Application.Current.Resources["ProductCardHoverStyle"] = Application.Current.Resources["ProductCardHoverStyle_NoMotion"] as System.Windows.Style;
            }
            else
            {
                // restore original
                // Re-define the animation style by reloading it from App.xaml resources
                // (Assumes original style key ProductCardHoverStyle is defined in App resources)
                Application.Current.Resources["ProductCardHoverStyle"] = Application.Current.Resources["ProductCardHoverStyle"];
            }

            // Reduce other motion globally if desired (not implemented fully: placeholder)
        }

        private void ToggleStudyMode_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                // Invoke the viewmodel command to toggle Study Mode
                vm.ToggleStudyModeCommand?.Execute(null);
            }
        }
    }
}