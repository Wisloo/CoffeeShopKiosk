using System;
using System.Collections.Generic;

namespace CoffeeShopKiosk.Models
{
    public class OrderModel
    {
        public int OrderId { get; set; }
        public List<ProductModel> Products { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime OrderDate { get; set; }

        public OrderModel()
        {
            Products = new List<ProductModel>();
        }
    }
}