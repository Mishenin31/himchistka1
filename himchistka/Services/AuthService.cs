using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using himchistka.Models;

namespace himchistka.Services
{
    public sealed class AuthService
    {
        private readonly List<User> _users = new List<User>();
        private int _nextId = 1;

        public AuthService()
        {
            SeedUsers();
        }

        public IReadOnlyList<User> Users => _users;

        public User Register(User user)
        {
            NormalizeUserFields(user);
            ValidationService.ValidateRegistration(user);
            if (_users.Any(u => string.Equals(u.Email, user.Email, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException("Пользователь с таким email уже существует.");
            if (_users.Any(u => u.Phone == user.Phone))
                throw new InvalidOperationException("Пользователь с таким телефоном уже существует.");

            user.Id = _nextId++;
            user.Role = UserRole.User;
            user.Password = HashPassword(user.Password);
            _users.Add(user);
            return user;
        }

        public User Login(string login, string password)
        {
            var normalizedLogin = (login ?? string.Empty).Trim();
            var normalizedPhoneLogin = NormalizePhone(normalizedLogin);
            var hashedPassword = HashPassword(password);
            var user = _users.FirstOrDefault(u =>
                (string.Equals(u.Email, normalizedLogin, StringComparison.OrdinalIgnoreCase) || u.Phone == normalizedPhoneLogin)
                && u.Password == hashedPassword);

            if (user == null)
                throw new UnauthorizedAccessException("Неверный логин или пароль.");

            return user;
        }

        public void UpdateProfile(User source, string fullName, string email, string phone, string password)
        {
            source.FullName = fullName?.Trim();
            source.Email = email?.Trim();
            source.Phone = NormalizePhone(phone);
            ValidationService.ValidateProfile(source);

            if (_users.Any(u => u.Id != source.Id && string.Equals(u.Email, source.Email, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException("Этот email уже используется другим пользователем.");
            if (_users.Any(u => u.Id != source.Id && u.Phone == source.Phone))
                throw new InvalidOperationException("Этот телефон уже используется другим пользователем.");

            if (!string.IsNullOrWhiteSpace(password))
            {
                var normalizedPassword = password.Trim();
                ValidationService.ValidatePassword(normalizedPassword);
                source.Password = HashPassword(normalizedPassword);
            }
        }

        public void UpdateUserRole(int userId, UserRole role)
        {
            var user = _users.FirstOrDefault(u => u.Id == userId) ?? throw new InvalidOperationException("Пользователь не найден.");
            user.Role = role;
        }

        private void SeedUsers()
        {
            _users.Add(new User { Id = _nextId++, FullName = "Администратор", Email = "admin@dryclean.local", Phone = "+79990000001", Password = HashPassword("admin123"), Role = UserRole.Administrator });
            _users.Add(new User { Id = _nextId++, FullName = "Менеджер", Email = "manager@dryclean.local", Phone = "+79990000002", Password = HashPassword("manager123"), Role = UserRole.Manager });
            _users.Add(new User { Id = _nextId++, FullName = "Клиент", Email = "user@dryclean.local", Phone = "+79990000003", Password = HashPassword("user123"), Role = UserRole.User });
        }

        private static void NormalizeUserFields(User user)
        {
            if (user == null) return;
            user.FullName = user.FullName?.Trim();
            user.Email = user.Email?.Trim();
            user.Phone = NormalizePhone(user.Phone);
            user.Password = user.Password?.Trim();
        }

        private static string NormalizePhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return string.Empty;

            var digits = new string(phone.Where(char.IsDigit).ToArray());
            if (digits.Length == 11 && digits.StartsWith("8", StringComparison.Ordinal))
                digits = string.Concat("7", digits.Substring(1));

            return digits.Length > 0 ? $"+{digits}" : string.Empty;
        }

        private static string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password ?? string.Empty);
                var hash = sha256.ComputeHash(bytes);
                var builder = new StringBuilder(hash.Length * 2);
                foreach (var b in hash)
                    builder.Append(b.ToString("x2", CultureInfo.InvariantCulture));
                return builder.ToString();
            }
        }
    }
}
