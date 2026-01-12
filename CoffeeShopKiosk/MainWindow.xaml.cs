
using System;
using System.Windows;
using CoffeeShopKiosk.ViewModels;
using System.Windows.Media;
using CoffeeShopKiosk.Services;
using System.Windows.Input;
using System.Windows.Shapes;

namespace CoffeeShopKiosk
{
    public partial class MainWindow : Window
    {
        private Services.ToastService _toastService;
        private System.Windows.Style _originalProductCardHoverStyle;
        private System.Windows.Style _originalAddButtonAnimatedStyle;



        public MainWindow()
        {
            InitializeComponent();
            var vm = new MainViewModel();
            DataContext = vm;

            // Wire up toast service to UI first so the VM change handler can safely show toasts
            _toastService = new ToastService();
            ToastHost.DataContext = _toastService;

            // Cache original hover style so we can restore it when toggling visual modes
            _originalProductCardHoverStyle = Application.Current.Resources["ProductCardHoverStyle"] as Style;
            _originalAddButtonAnimatedStyle = Application.Current.Resources["AddButtonAnimated"] as Style;





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
            ThemeSelector.ToolTip = "Select visual theme";
            OpenStudyAssistant.Click += (s,e) =>
            {
                var settings = new SettingsService();
                if (!settings.Settings.EnableAIStudyAssistant)
                {
                    var res = System.Windows.MessageBox.Show("AI Study Assistant is currently disabled in Visual Settings. Enable it to use this feature.", "AI Disabled", System.Windows.MessageBoxButton.OKCancel);
                    if (res == System.Windows.MessageBoxResult.OK)
                    {
                        var settingsDlg = new Views.StudySettings() { Owner = this };
                        settingsDlg.ShowDialog();
                        settings = new SettingsService();
                        if (!settings.Settings.EnableAIStudyAssistant) return;
                    }
                    else
                    {
                        return;
                    }
                }

                var dlg = new Views.StudyAssistant() { Owner = this };
                dlg.ShowDialog();
            };

            OpenVisualSettings.Click += (s,e) =>
            {
                var dlg = new Views.StudySettings() { Owner = this };
                var result = dlg.ShowDialog();
                if (result == true)
                {
                    // After settings change, re-apply visuals and UI toggles
                    var settings = new SettingsService();
                    SpotlightOverlay.Show(settings.Settings.SpotlightEnabled, 0.54);
                    if (DataContext is MainViewModel mainVm)
                    {
                        mainVm.IsHideProductImages = settings.Settings.HideProductImages;
                    }

                    // Apply the selected visual theme
                    ApplyTheme(settings.Settings.VisualTheme);
                    ThemeSelector.SelectedValue = settings.Settings.VisualTheme;

                    // Update theme visuals
                    if (settings.Settings.VisualEffectsEnabled)
                    {
                        ThemeVisuals.SetTheme(settings.Settings.VisualTheme);
                    }
                    else
                    {
                        ThemeVisuals.StopEffects();
                    }

                    // Update Study Assistant availability UI
                    OpenStudyAssistant.IsEnabled = settings.Settings.EnableAIStudyAssistant;
                    OpenStudyAssistant.ToolTip = settings.Settings.EnableAIStudyAssistant ? "Study Assistant (Ctrl+I)" : "Study Assistant is disabled — enable in Visual Settings";
                }
            };

            // Initialize from settings
            var settings = new SettingsService();

            // Enable/disable Study button based on opt-in setting
            OpenStudyAssistant.IsEnabled = settings.Settings.EnableAIStudyAssistant;
            OpenStudyAssistant.ToolTip = settings.Settings.EnableAIStudyAssistant ? "Study Assistant (Ctrl+I)" : "Study Assistant is disabled — enable in Visual Settings";

            // Apply visual theme
            ApplyTheme(settings.Settings.VisualTheme);
            ThemeSelector.SelectedValue = settings.Settings.VisualTheme;

            // Handle theme changes from the header selector
            ThemeSelector.SelectionChanged += (ss, ee) =>
            {
                var val = (ThemeSelector.SelectedValue ?? "Cafe").ToString();
                ApplyTheme(val);
                ThemeVisuals.SetTheme(val);
            };
            _toastService.Muted = settings.Settings.DoNotDisturb;

            // Initialize theme visuals based on the selected theme and whether effects are enabled and accessibility settings
            if (settings.Settings.VisualEffectsEnabled && !settings.Settings.ReduceMotion)
            {
                ThemeVisuals.SetTheme(settings.Settings.VisualTheme);
            }
            else
            {
                ThemeVisuals.StopEffects();
            }

            // Respect image hiding and spotlight toggles
            if (DataContext is MainViewModel mainVm)
            {
                mainVm.IsHideProductImages = settings.Settings.HideProductImages;
            }

            if (settings.Settings.SpotlightEnabled)
            {
                SpotlightOverlay.Show(true, 0.54);
            }
            else
            {
                SpotlightOverlay.Show(false);
            }


        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.I && (System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Control) == System.Windows.Input.ModifierKeys.Control)
            {
                // Ctrl+I pressed — open study assistant (respecting opt-in setting)
                var settings = new SettingsService();
                if (!settings.Settings.EnableAIStudyAssistant)
                {
                    var res = System.Windows.MessageBox.Show("AI Study Assistant is currently disabled in Visual Settings. Enable it to use this feature.", "AI Disabled", System.Windows.MessageBoxButton.OKCancel);
                    if (res == System.Windows.MessageBoxResult.OK)
                    {
                        var settingsDlg = new Views.StudySettings() { Owner = this };
                        settingsDlg.ShowDialog();
                        settings = new SettingsService();
                        if (!settings.Settings.EnableAIStudyAssistant) return;
                    }
                    else
                    {
                        return;
                    }
                }

                var assistant = new Views.StudyAssistant() { Owner = this };
                assistant.ShowDialog();
            }
        }

        private void ApplyTheme(string theme)
        {
            // Replace brushes based on the selected theme
            var appRes = Application.Current.Resources;
            var t = (theme ?? "Cafe").ToLower();

            switch (t)
            {
                case "rain":
                    appRes["CoffeeBrown"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2B3A66"));
                    appRes["Cream"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F4F7FA"));
                    appRes["CardBackground"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EEF3FA"));
                    break;
                case "sunset":
                    appRes["CoffeeBrown"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7A2E3A"));
                    appRes["Cream"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF4EE"));
                    appRes["CardBackground"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF6F0"));
                    break;
                case "coffeeshop":
                    appRes["CoffeeBrown"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4B2E19"));
                    appRes["Cream"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF8F0"));
                    appRes["CardBackground"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF6EE"));
                    break;
                case "minimal":
                    appRes["CoffeeBrown"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#222222"));
                    appRes["Cream"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF"));
                    appRes["CardBackground"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FAFAFA"));
                    break;
                default: // cafe (brighter, warm palette)
                    appRes["CoffeeBrown"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#5B3A2A"));
                    appRes["Cream"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF9F6"));
                    appRes["CardBackground"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF8F3"));
                    appRes["AccentGreen"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#36B37E"));
                    break;
            }

            // Persist the choice
            var settingsSvc = new SettingsService();
            settingsSvc.Update(s => s.VisualTheme = theme ?? "Cafe");
        }




    }
}