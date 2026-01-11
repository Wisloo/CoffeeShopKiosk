using System.Collections.Generic;
using CoffeeShopKiosk.Models;

namespace CoffeeShopKiosk.Services
{
    public interface IOrderService
    {
        void SaveOrder(OrderModel order);
        List<OrderModel> GetAllOrders();
    }
}