using System.Security.Cryptography;
using System.Text;
using Airport.Models;
using Airport.Data;
using Airport.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Airport.Services
{
    public interface IAuthService
    {
        Task<User> RegisterAsync(RegisterViewModel model);
        Task<User> LoginAsync(LoginViewModel model);
        Task<bool> UserExistsAsync(string email);
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
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private bool VerifyPassword(string password, string hash)
        {
            return HashPassword(password) == hash;
        }
    }
} 