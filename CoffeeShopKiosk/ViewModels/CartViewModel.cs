using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using CoffeeShopKiosk.Models;
using CoffeeShopKiosk.Services;

namespace CoffeeShopKiosk.ViewModels
{
    public class CartViewModel : BaseViewModel
    {
        private readonly IOrderService _orderService;

        public ObservableCollection<CartItem> Items { get; } = new ObservableCollection<CartItem>();

        public decimal TotalAmount => Items.Sum(i => i.LineTotal);

        public ICommand RemoveItemCommand { get; }
        public ICommand CheckoutCommand { get; }

        public event Action<OrderModel> OrderPlaced;

        public CartViewModel(IOrderService orderService)
        {
            _orderService = orderService;
            RemoveItemCommand = new RelayCommand<CartItem>(RemoveItem);
            CheckoutCommand = new RelayCommand<object>(o => Checkout(), o => Items.Any());
        }

        public void AddToCart(ProductModel product)
        {
            if (product == null) return;
            var existing = Items.FirstOrDefault(i => i.Product.ProductId == product.ProductId);
            if (existing != null)
            {
                existing.Quantity++;
                OnPropertyChanged(nameof(TotalAmount));
            }
            else
            {
                Items.Add(new CartItem { Product = product, Quantity = 1 });
                OnPropertyChanged(nameof(TotalAmount));
            }
        }

        private void RemoveItem(CartItem item)
        {
            if (item == null) return;
            Items.Remove(item);
            OnPropertyChanged(nameof(TotalAmount));
        }

        public void Checkout()
        {
            var order = new OrderModel();
            foreach (var it in Items)
            {
                order.Items.Add(new CartItem { Product = it.Product, Quantity = it.Quantity });
            }
            order.TotalAmount = TotalAmount;
            _orderService.SaveOrder(order);
            OrderPlaced?.Invoke(order);
            Items.Clear();
            OnPropertyChanged(nameof(TotalAmount));
        }
    }
}