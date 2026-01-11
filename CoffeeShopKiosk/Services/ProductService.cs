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
                new ProductModel { ProductId = 4, Name = "Muffin", Description = "Soft and sweet muffin", Price = 1.50m, ImageUrl = "images/muffin.jpg" }
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