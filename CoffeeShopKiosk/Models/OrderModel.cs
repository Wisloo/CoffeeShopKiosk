using System;
using System.Collections.Generic;
using System.Linq;

namespace CoffeeShopKiosk.Models
{
    public class OrderModel
    {
        public int OrderId { get; set; }

        // New: cart items to preserve quantity information
        public List<CartItem> Items { get; set; } = new List<CartItem>();

        // Keep Products for compatibility (flattened list)
        public List<ProductModel> Products => Items?.SelectMany(i => Enumerable.Repeat(i.Product, i.Quantity)).ToList();

        public decimal TotalAmount { get; set; }
        public DateTime OrderDate { get; set; }

        public OrderModel()
        {
            Items = new List<CartItem>();
        }
    }
}