using System;
using System.Text.RegularExpressions;
using himchistka.Models;

namespace himchistka.Services
{
    public static class ValidationService
    {
        private static readonly Regex EmailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);
        private static readonly Regex PhoneRegex = new Regex(@"^\+?[0-9]{10,15}$", RegexOptions.Compiled);

        public static void ValidateRegistration(User user)
        {
            if (user == null) throw new ArgumentException("Пользователь не может быть пустым.");
            if (string.IsNullOrWhiteSpace(user.FullName)) throw new ArgumentException("Введите имя.");
            if (string.IsNullOrWhiteSpace(user.Email) || !EmailRegex.IsMatch(user.Email)) throw new ArgumentException("Некорректный email.");
            if (string.IsNullOrWhiteSpace(user.Phone) || !PhoneRegex.IsMatch(user.Phone)) throw new ArgumentException("Некорректный телефон. Пример: +79991234567");
            if (string.IsNullOrWhiteSpace(user.Password) || user.Password.Length < 6) throw new ArgumentException("Пароль должен содержать минимум 6 символов.");
        }

        public static void ValidateProduct(Product product)
        {
            if (product == null) throw new ArgumentException("Товар не может быть пустым.");
            if (string.IsNullOrWhiteSpace(product.Name)) throw new ArgumentException("Название товара обязательно.");
            if (string.IsNullOrWhiteSpace(product.Category)) throw new ArgumentException("Категория обязательна.");
            if (product.Price < 0 || product.Price > 100000) throw new ArgumentException("Цена должна быть в диапазоне 0..100000.");

            if (product.OldPrice.HasValue && (product.OldPrice < 0 || product.OldPrice > 100000))
                throw new ArgumentException("Старая цена должна быть в диапазоне 0..100000.");

            if (product.DiscountPercent.HasValue && (product.DiscountPercent < 1 || product.DiscountPercent > 99))
                throw new ArgumentException("Скидка должна быть в диапазоне 1..99%.");
        }
    }
}
