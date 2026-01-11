using System;

namespace CoffeeShopKiosk.Models
{
    public class CartItem
    {
        public ProductModel Product { get; set; }
        public int Quantity { get; set; } = 1;

        public decimal LineTotal => (Product?.Price ?? 0m) * Quantity;
    }
}