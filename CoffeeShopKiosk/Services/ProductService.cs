using System.Collections.Generic;
using System.Linq;
using CoffeeShopKiosk.Models;

namespace CoffeeShopKiosk.Services
{
    public class ProductService
    {
        private List<ProductModel> _products;

        public ProductService()
        {
            // Sample data for demonstration purposes
            _products = new List<ProductModel>
            {
                new ProductModel { ProductId = 1, Name = "Espresso", Description = "Strong and bold coffee", Price = 2.50m, ImageUrl = "images/espresso.jpg" },
                new ProductModel { ProductId = 2, Name = "Cappuccino", Description = "Coffee with steamed milk and foam", Price = 3.00m, ImageUrl = "images/cappuccino.jpg" },
                new ProductModel { ProductId = 3, Name = "Croissant", Description = "Flaky and buttery pastry", Price = 2.00m, ImageUrl = "images/croissant.jpg" },
                new ProductModel { ProductId = 4, Name = "Muffin", Description = "Soft and sweet muffin", Price = 1.50m, ImageUrl = "images/muffin.jpg" },
                new ProductModel { ProductId = 5, Name = "Latte", Description = "Smooth espresso with steamed milk", Price = 3.50m, ImageUrl = "images/latte.jpg" },
                new ProductModel { ProductId = 6, Name = "Americano", Description = "Espresso with hot water", Price = 2.75m, ImageUrl = "images/americano.jpg" },
                new ProductModel { ProductId = 7, Name = "Mocha", Description = "Espresso with chocolate and steamed milk", Price = 3.75m, ImageUrl = "images/mocha.jpg" },
                new ProductModel { ProductId = 8, Name = "Bagel", Description = "Freshly baked bagel with cream cheese", Price = 2.25m, ImageUrl = "images/bagel.jpg" },
                new ProductModel { ProductId = 9, Name = "Blueberry Muffin", Description = "Muffin with fresh blueberries", Price = 2.00m, ImageUrl = "images/blueberry_muffin.jpg" },
                new ProductModel { ProductId = 10, Name = "Macchiato", Description = "Espresso with a dash of foamed milk", Price = 3.25m, ImageUrl = "images/macchiato.jpg" },
                new ProductModel { ProductId = 11, Name = "Flat White", Description = "Espresso with microfoam", Price = 3.20m, ImageUrl = "images/flatwhite.jpg" },
                new ProductModel { ProductId = 12, Name = "Chocolate Cake", Description = "Rich and moist chocolate cake slice", Price = 2.80m, ImageUrl = "images/chocolate_cake.jpg" }
            };
        }

        public List<ProductModel> GetAllProducts()
        {
            return _products;
        }

        public ProductModel GetProductById(int productId)
        {
            return _products.FirstOrDefault(p => p.ProductId == productId);
        }
    }
}