using System;
using System.Collections.Generic;
using System.Linq;
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
            ValidationService.ValidateRegistration(user);
            if (_users.Any(u => string.Equals(u.Email, user.Email, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException("Пользователь с таким email уже существует.");
            if (_users.Any(u => u.Phone == user.Phone))
                throw new InvalidOperationException("Пользователь с таким телефоном уже существует.");

            user.Id = _nextId++;
            user.Role = UserRole.User;
            _users.Add(user);
            return user;
        }

        public User Login(string login, string password)
        {
            var user = _users.FirstOrDefault(u =>
                (string.Equals(u.Email, login, StringComparison.OrdinalIgnoreCase) || u.Phone == login)
                && u.Password == password);

            if (user == null)
                throw new UnauthorizedAccessException("Неверный логин или пароль.");

            return user;
        }

        public void UpdateProfile(User source, string fullName, string email, string phone, string password)
        {
            source.FullName = fullName?.Trim();
            source.Email = email?.Trim();
            source.Phone = phone?.Trim();
            source.Password = password;
            ValidationService.ValidateRegistration(source);
        }

        public void UpdateUserRole(int userId, UserRole role)
        {
            var user = _users.FirstOrDefault(u => u.Id == userId) ?? throw new InvalidOperationException("Пользователь не найден.");
            user.Role = role;
        }

        private void SeedUsers()
        {
            _users.Add(new User { Id = _nextId++, FullName = "Администратор", Email = "admin@dryclean.local", Phone = "+79990000001", Password = "admin123", Role = UserRole.Administrator });
            _users.Add(new User { Id = _nextId++, FullName = "Менеджер", Email = "manager@dryclean.local", Phone = "+79990000002", Password = "manager123", Role = UserRole.Manager });
            _users.Add(new User { Id = _nextId++, FullName = "Клиент", Email = "user@dryclean.local", Phone = "+79990000003", Password = "user123", Role = UserRole.User });
        }
    }
}
