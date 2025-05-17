using System.Security.Cryptography;
using System.Text;
using Airport.Models;
using Airport.Data;
using Airport.ViewModels;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace Airport.Services
{
    public interface IAuthService
    {
        Task<User> RegisterAsync(RegisterViewModel model);
        Task<User> LoginAsync(LoginViewModel model);
        Task<bool> UserExistsAsync(string email);
        Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
        Task<User> UpdateProfileAsync(int userId, EditProfileViewModel model);
    }

    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;

        public AuthService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User> RegisterAsync(RegisterViewModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            // Валидация входных данных
            if (string.IsNullOrEmpty(model.Email))
            {
                throw new ArgumentException("Email не может быть пустым", nameof(model.Email));
            }

            if (string.IsNullOrEmpty(model.Username))
            {
                throw new ArgumentException("Имя пользователя не может быть пустым", nameof(model.Username));
            }

            if (string.IsNullOrEmpty(model.Password))
            {
                throw new ArgumentException("Пароль не может быть пустым", nameof(model.Password));
            }

            if (await UserExistsAsync(model.Email))
            {
                throw new Exception("Пользователь с таким email уже существует");
            }

            var user = new User
            {
                Username = model.Username,
                Email = model.Email,
                PasswordHash = HashPassword(model.Password)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<User> LoginAsync(LoginViewModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            if (string.IsNullOrEmpty(model.Email))
            {
                throw new ArgumentException("Email не может быть пустым", nameof(model.Email));
            }

            if (string.IsNullOrEmpty(model.Password))
            {
                throw new ArgumentException("Пароль не может быть пустым", nameof(model.Password));
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == model.Email);

            if (user == null || !VerifyPassword(model.Password, user.PasswordHash))
            {
                throw new Exception("Неверный email или пароль");
            }

            return user;
        }

        public async Task<bool> UserExistsAsync(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                throw new ArgumentException("Email не может быть пустым", nameof(email));
            }

            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            if (string.IsNullOrEmpty(currentPassword))
            {
                throw new ArgumentException("Текущий пароль не может быть пустым");
            }

            if (string.IsNullOrEmpty(newPassword))
            {
                throw new ArgumentException("Новый пароль не может быть пустым");
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new Exception("Пользователь не найден");
            }

            if (!VerifyPassword(currentPassword, user.PasswordHash))
            {
                throw new Exception("Текущий пароль указан неверно");
            }

            user.PasswordHash = HashPassword(newPassword);
            await _context.SaveChangesAsync();
            
            return true;
        }

        public async Task<User> UpdateProfileAsync(int userId, EditProfileViewModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            if (string.IsNullOrEmpty(model.Email))
            {
                throw new ArgumentException("Email не может быть пустым", nameof(model.Email));
            }

            if (string.IsNullOrEmpty(model.Username))
            {
                throw new ArgumentException("Имя пользователя не может быть пустым", nameof(model.Username));
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new Exception("Пользователь не найден");
            }

            // Проверяем, не занят ли email другим пользователем, если он был изменен
            if (user.Email != model.Email && await _context.Users.AnyAsync(u => u.Email == model.Email && u.Id != userId))
            {
                throw new Exception("Пользователь с таким email уже существует");
            }

            user.Username = model.Username;
            user.Email = model.Email;

            await _context.SaveChangesAsync();
            return user;
        }

        private string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("Пароль не может быть пустым", nameof(password));
            }

            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        private bool VerifyPassword(string password, string hash)
        {
            if (string.IsNullOrEmpty(password))
            {
                return false;
            }

            if (string.IsNullOrEmpty(hash))
            {
                return false;
            }

            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
    }
} 