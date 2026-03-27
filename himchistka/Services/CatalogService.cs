using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using himchistka.Models;

namespace himchistka.Services
{
    public sealed class CatalogService
    {
        private readonly DatabaseService _database;
        private readonly List<Product> _products;
        private readonly List<Order> _orders;

        public CatalogService(DatabaseService database)
        {
            _database = database ?? throw new ArgumentNullException(nameof(database));
            _products = _database.State.Products;
            _orders = _database.State.Orders;
            SeedProductsIfEmpty();
        }

        public IReadOnlyList<Product> Products => _products;
        public IReadOnlyList<Order> Orders => _orders;

        public IEnumerable<Product> QueryProducts(string search, string sort)
        {
            var query = _products.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                var normalized = search.Trim().ToLowerInvariant();
                query = query.Where(p => p.Name.ToLowerInvariant().Contains(normalized) || p.Category.ToLowerInvariant().Contains(normalized));
            }

            switch (sort)
            {
                case "Цена по возрастанию": query = query.OrderBy(p => p.EffectivePrice); break;
                case "Цена по убыванию": query = query.OrderByDescending(p => p.EffectivePrice); break;
                case "Название": query = query.OrderBy(p => p.Name); break;
            }

            return query.ToList();
        }

        public Product AddProduct(Product product)
        {
            ValidationService.ValidateProduct(product);
            product.Id = _database.State.NextProductId++;
            _products.Add(product);
            _database.Save();
            return product;
        }

        public void UpdateProduct(Product changed)
        {
            ValidationService.ValidateProduct(changed);
            var product = _products.FirstOrDefault(p => p.Id == changed.Id) ?? throw new InvalidOperationException("Товар не найден.");
            product.Name = changed.Name;
            product.Category = changed.Category;
            product.Price = changed.Price;
            product.OldPrice = changed.OldPrice;
            product.DiscountPercent = changed.DiscountPercent;
            product.ImagePath = changed.ImagePath;
            _database.Save();
        }

        public void DeleteProduct(int productId)
        {
            if (_orders.Any(o => o.Items.Any(i => i.ProductId == productId)))
                throw new InvalidOperationException("Невозможно удалить товар, так как он присутствует в одном или нескольких заказах.");

            var product = _products.FirstOrDefault(p => p.Id == productId) ?? throw new InvalidOperationException("Товар не найден.");
            _products.Remove(product);
            _database.Save();
        }

        public Order Checkout(int userId, List<OrderItem> items, DateTime scheduledAt, int durationHours, bool isQueueBooking)
        {
            if (items == null || items.Count == 0)
                throw new InvalidOperationException("Корзина пуста.");
            if (durationHours <= 0)
                throw new InvalidOperationException("Некорректная длительность записи.");
            if (scheduledAt < DateTime.Now.AddHours(-1))
                throw new InvalidOperationException("Нельзя выбрать время записи в прошлом.");

            var totalAmount = items.Sum(i => i.Total);
            var queueDeposit = isQueueBooking
                ? Math.Round(totalAmount * 0.08m, 2)
                : 0m;

            var order = new Order
            {
                Id = _database.State.NextOrderId++,
                UserId = userId,
                CreatedAt = DateTime.Now,
                ScheduledAt = scheduledAt,
                DurationHours = durationHours,
                IsQueueBooking = isQueueBooking,
                QueueDepositAmount = queueDeposit,
                Items = items.Select(i => new OrderItem
                {
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    UnitPrice = i.UnitPrice,
                    Quantity = i.Quantity
                }).ToList(),
                TotalAmount = totalAmount
            };

            _orders.Add(order);
            _database.Save();
            return order;
        }

        public string SaveImageToProject(string sourceFile)
        {
            if (string.IsNullOrWhiteSpace(sourceFile) || !File.Exists(sourceFile))
                throw new InvalidOperationException("Файл изображения не найден.");

            var productImagesDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ProductImages");
            Directory.CreateDirectory(productImagesDir);

            var extension = Path.GetExtension(sourceFile);
            var fileName = $"img_{DateTime.Now:yyyyMMddHHmmssfff}{extension}";
            var destination = Path.Combine(productImagesDir, fileName);
            File.Copy(sourceFile, destination, false);

            return Path.Combine("ProductImages", fileName);
        }

        private void SeedProductsIfEmpty()
        {
            if (_products.Count > 0)
                return;

            AddProduct(new Product { Name = "Пальто шерстяное", Category = "Одежда", Price = 1200, OldPrice = 1500, DiscountPercent = 20 });
            AddProduct(new Product { Name = "Костюм мужской", Category = "Одежда", Price = 950 });
            AddProduct(new Product { Name = "Химчистка ковра", Category = "Дом", Price = 2200 });
            AddProduct(new Product { Name = "Чистка обуви", Category = "Обувь", Price = 600 });
        }
    }
}
