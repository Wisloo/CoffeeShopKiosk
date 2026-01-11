using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using CoffeeShopKiosk.Models;

namespace CoffeeShopKiosk.Services
{
    public class OrderService : IOrderService
    {
        private readonly string _path;
        private readonly object _lock = new object();

        public OrderService()
        {
            var dir = AppDomain.CurrentDomain.BaseDirectory;
            _path = Path.Combine(dir, "orders.json");
        }

        public void SaveOrder(OrderModel order)
        {
            lock (_lock)
            {
                var orders = GetAllOrders();
                order.OrderId = (orders.Any() ? orders.Max(o => o.OrderId) : 0) + 1;
                order.OrderDate = DateTime.UtcNow;
                order.TotalAmount = order.Items?.Sum(i => i.LineTotal) ?? 0m;
                orders.Add(order);

                var json = JsonSerializer.Serialize(orders, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_path, json);
            }
        }

        public List<OrderModel> GetAllOrders()
        {
            lock (_lock)
            {
                if (!File.Exists(_path)) return new List<OrderModel>();
                try
                {
                    var json = File.ReadAllText(_path);
                    return JsonSerializer.Deserialize<List<OrderModel>>(json) ?? new List<OrderModel>();
                }
                catch
                {
                    return new List<OrderModel>();
                }
            }
        }
    }
}