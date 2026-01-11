using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CoffeeShopKiosk.Models
{
    public class CartItem : INotifyPropertyChanged
    {
        private int _quantity = 1;

        public ProductModel Product { get; set; }

        public int Quantity
        {
            get => _quantity;
            set
            {
                if (_quantity == value) return;
                _quantity = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(LineTotal));
            }
        }

        public decimal LineTotal => (Product?.Price ?? 0m) * Quantity;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}