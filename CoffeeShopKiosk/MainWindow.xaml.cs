
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
        private System.Windows.Style _originalProductCardHoverStyle;
        private System.Windows.Style _originalAddButtonAnimatedStyle;
        private bool _previousToastMuted;

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

            // Cache original hover style so we can restore it when leaving Study Mode
            _originalProductCardHoverStyle = Application.Current.Resources["ProductCardHoverStyle"] as Style;
            _originalAddButtonAnimatedStyle = Application.Current.Resources["AddButtonAnimated"] as Style;

            // Listen for ViewModel changes so UI can react when Study Mode is toggled via keyboard shortcut or other means
            vm.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(vm.IsStudyMode))
                {
                    // Apply the theme and show a small toast for discoverability
                    ApplyStudyMode(vm.IsStudyMode);

                    // Spotlight & Image hiding based on settings
                    var settings = new SettingsService();
                    if (vm.IsStudyMode && settings.Settings.SpotlightEnabled)
                    {
                        SpotlightOverlay.Show(true, 0.54);
                    }
                    else
                    {
                        SpotlightOverlay.Show(false);
                    }

                    if (vm.IsStudyMode && settings.Settings.HideProductImages)
                    {
                        // Notify product list via ViewModel property
                        vm.IsHideProductImages = true;
                    }
                    else
                    {
                        vm.IsHideProductImages = false;
                    }

                    // Auto DND: save previous toast muted state and mute while study mode is on
                    if (vm.IsStudyMode)
                    {
                        _previousToastMuted = _toastService.Muted;
                        _toastService.Muted = true; // mute while studying

                        // Optionally auto-start Pomodoro when Study Mode is enabled (default true)
                        if (settings.Settings.AutoStartPomodoroOnStudyMode)
                        {
                            vm.Pomodoro?.Start();
                            _toastService?.Show("Pomodoro started");
                        }
                        // Optionally start ambient sound
                        if (settings.Settings.AmbientSoundEnabled)
                        {
                            AmbientSoundService.Play(settings.Settings.AmbientSoundChoice, settings.Settings.AmbientSoundVolume);
                        }
                    }
                    else
                    {
                        // Restore previous DND setting
                        _toastService.Muted = _previousToastMuted;
                        vm.Pomodoro?.Pause();
                        AmbientSoundService.Stop();
                        _toastService?.Show("Study Mode disabled");
                    }
                }
            };

            // Register command binding for the routed ToggleStudyMode command
            CommandBindings.Add(new CommandBinding(ToggleStudyModeRoutedCommand, ToggleStudyMode_Executed));

            // Show a toast when Pomodoro period switches (work <-> break)
            vm.Pomodoro.PeriodSwitched += isWork =>
            {
                _toastService?.Show(isWork ? "Work session started" : "Break started");
            };

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
            OpenStudySettings.Click += (s,e) =>
            {
                var dlg = new Views.StudySettings() { Owner = this };
                var result = dlg.ShowDialog();
                if (result == true)
                {
                    // After settings change, re-apply Study Mode side-effects if active
                    var settings = new SettingsService();
                    if (vm.IsStudyMode)
                    {
                        SpotlightOverlay.Show(settings.Settings.SpotlightEnabled, 0.54);
                        vm.IsHideProductImages = settings.Settings.HideProductImages;
                        if (settings.Settings.AmbientSoundEnabled)
                        {
                            AmbientSoundService.Play(settings.Settings.AmbientSoundChoice, settings.Settings.AmbientSoundVolume);
                        }
                        else
                        {
                            AmbientSoundService.Stop();
                        }
                    }
                }
            };

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
            // Replace brushes with fresh SolidColorBrush instances so DynamicResource users update immediately.
            var appRes = Application.Current.Resources;

            appRes["CoffeeBrown"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(study ? "#2B1A12" : "#4B2E19"));
            appRes["Cream"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(study ? "#F5EFE9" : "#FFF8F0"));
            appRes["CardBackground"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(study ? "#F6EEE8" : "#FFF6EE"));
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
                // disable add button animation as well when reducing motion
                if (Application.Current.Resources["AddButtonAnimated_NoMotion"] is System.Windows.Style noMotion)
                {
                    Application.Current.Resources["AddButtonAnimated"] = noMotion;
                }
            }
            else
            {
                // restore cached original style (so we don't lose the animated version)
                if (_originalProductCardHoverStyle != null)
                {
                    Application.Current.Resources["ProductCardHoverStyle"] = _originalProductCardHoverStyle;
                }
                if (_originalAddButtonAnimatedStyle != null)
                {
                    Application.Current.Resources["AddButtonAnimated"] = _originalAddButtonAnimatedStyle;
                }
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