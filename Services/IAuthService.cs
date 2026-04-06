using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using DinoApp.Models;
using Microsoft.AspNetCore.Http;

namespace DinoApp.Services;

public interface IAuthService
{
    Task<bool> LoginAsync(string username, string password, bool rememberMe);
    void Logout();
    User? GetCurrentUser();
    User? GetUserById(int id);
    List<User> GetAllUsers();
    bool IsAuthenticated();
    Task<bool> RegisterAsync(RegisterUserDto dto);
    Task<bool> UpdateProfileAsync(int userId, UpdateProfileDto dto);
}

public class AuthService : IAuthService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly string _usersFilePath;
    private List<User> _users;

    public AuthService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
        _usersFilePath = Path.Combine(Directory.GetCurrentDirectory(), "users.json");
        _users = LoadUsers();
    }

    private List<User> LoadUsers()
    {
        if (File.Exists(_usersFilePath))
        {
            var json = File.ReadAllText(_usersFilePath);
            return JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();
        }

        // Создаём администратора по умолчанию
        var defaultUsers = new List<User>
        {
            new User
            {
                Id = 1,
                Username = "admin",
                PasswordHash = HashPassword("admin123"),
                Email = "admin@dino-mir.com",
                FullName = "Администратор",
                Bio = "Я создатель этого удивительного мира динозавров!",
                AvatarUrl = "/images/default-avatar.png",
                CreatedAt = DateTime.Now,
                LastLoginAt = DateTime.Now,
                IsAdmin = true
            }
        };

        SaveUsers(defaultUsers);
        return defaultUsers;
    }

    private void SaveUsers(List<User> users)
    {
        var json = JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_usersFilePath, json);
    }

    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }

    public async Task<bool> LoginAsync(string username, string password, bool rememberMe)
    {
        var user = _users.FirstOrDefault(u => u.Username == username);

        if (user == null || user.PasswordHash != HashPassword(password))
            return false;

        var session = _httpContextAccessor.HttpContext?.Session;
        if (session != null)
        {
            session.SetString("UserId", user.Id.ToString());
            session.SetString("Username", user.Username);
            session.SetString("IsAdmin", user.IsAdmin.ToString());
            session.SetString("FullName", user.FullName ?? user.Username);
            session.SetString("AvatarUrl", user.AvatarUrl ?? "/images/default-avatar.png");

            // Обновляем время последнего входа
            user.LastLoginAt = DateTime.Now;
            SaveUsers(_users);
        }

        return await Task.FromResult(true);
    }

    public void Logout()
    {
        _httpContextAccessor.HttpContext?.Session.Clear();
    }

    public User? GetCurrentUser()
    {
        var userId = _httpContextAccessor.HttpContext?.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userId))
            return null;

        return _users.FirstOrDefault(u => u.Id.ToString() == userId);
    }

    public User? GetUserById(int id)
    {
        return _users.FirstOrDefault(u => u.Id == id);
    }

    public List<User> GetAllUsers()
    {
        return _users.ToList();
    }

    public bool IsAuthenticated()
    {
        return !string.IsNullOrEmpty(_httpContextAccessor.HttpContext?.Session.GetString("UserId"));
    }

    public async Task<bool> RegisterAsync(RegisterUserDto dto)
    {
        if (_users.Any(u => u.Username == dto.Username))
            return await Task.FromResult(false);

        if (_users.Any(u => u.Email == dto.Email))
            return await Task.FromResult(false);

        var newUser = new User
        {
            Id = _users.Count > 0 ? _users.Max(u => u.Id) + 1 : 1,
            Username = dto.Username,
            PasswordHash = HashPassword(dto.Password),
            Email = dto.Email,
            FullName = dto.FullName,
            Bio = "Любитель динозавров! 🦕",
            AvatarUrl = "/images/default-avatar.png",
            CreatedAt = DateTime.Now,
            IsAdmin = false
        };

        _users.Add(newUser);
        SaveUsers(_users);

        return await Task.FromResult(true);
    }

    public async Task<bool> UpdateProfileAsync(int userId, UpdateProfileDto dto)
    {
        var user = _users.FirstOrDefault(u => u.Id == userId);
        if (user == null)
            return false;

        if (!string.IsNullOrEmpty(dto.FullName))
            user.FullName = dto.FullName;

        if (!string.IsNullOrEmpty(dto.Bio))
            user.Bio = dto.Bio;

        if (!string.IsNullOrEmpty(dto.Email))
            user.Email = dto.Email;

        if (!string.IsNullOrEmpty(dto.AvatarUrl))
            user.AvatarUrl = dto.AvatarUrl;

        user.UpdatedAt = DateTime.Now;

        SaveUsers(_users);

        // Обновляем сессию если это текущий пользователь
        var session = _httpContextAccessor.HttpContext?.Session;
        if (session != null && session.GetString("UserId") == userId.ToString())
        {
            session.SetString("FullName", user.FullName ?? user.Username);
            if (!string.IsNullOrEmpty(dto.AvatarUrl))
                session.SetString("AvatarUrl", dto.AvatarUrl);
        }

        return await Task.FromResult(true);
    }
}