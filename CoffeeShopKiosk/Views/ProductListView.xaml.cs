using System;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media.Imaging;
using CoffeeShopKiosk.Models;

namespace CoffeeShopKiosk.Views
{
    public partial class ProductListView : UserControl
    {
        public ProductListView()
        {
            InitializeComponent();
        }

        private void Image_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            try
            {
                var img = sender as Image;
                var prod = img?.DataContext as ProductModel;
                System.Diagnostics.Debug.WriteLine($"Image failed for product {prod?.Name}. Source={img?.Source}, Error={e.ErrorException}");

                if (prod != null && prod.ImageUrl != null && prod.ImageUrl.StartsWith("pack://siteoforigin:"))
                {
                    var prefix = "pack://siteoforigin:,,,/";
                    var relative = prod.ImageUrl.Substring(prefix.Length);
                    var exeDir = AppDomain.CurrentDomain.BaseDirectory;
                    var filePath = System.IO.Path.Combine(exeDir, relative.Replace('/', System.IO.Path.DirectorySeparatorChar));
                    System.Diagnostics.Debug.WriteLine($"Checking file exists: {filePath} -> {System.IO.File.Exists(filePath)}");

                    if (System.IO.File.Exists(filePath))
                    {
                        img.Source = new BitmapImage(new Uri(filePath));
                        System.Diagnostics.Debug.WriteLine($"Loaded image from file path for {prod.Name}");
                        e.Handled = true;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Image_ImageFailed handler exception: {ex}");
            }
        }
    }
}
