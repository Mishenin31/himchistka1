using System;
using System.Collections.Generic;

namespace himchistka.Models
{
    public enum UserRole
    {
        Guest,
        User,
        Manager,
        Administrator
    }

    public sealed class User
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Password { get; set; }
        public UserRole Role { get; set; }
    }

    public sealed class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public decimal Price { get; set; }
        public decimal? OldPrice { get; set; }
        public int? DiscountPercent { get; set; }
        public string ImagePath { get; set; }

        public decimal EffectivePrice => (OldPrice.HasValue && DiscountPercent.HasValue)
            ? Math.Round(OldPrice.Value * (100 - DiscountPercent.Value) / 100m, 2)
            : Price;
    }

    public sealed class OrderItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal Total => UnitPrice * Quantity;
    }

    public sealed class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<OrderItem> Items { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
