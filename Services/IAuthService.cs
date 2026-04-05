using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using DinoApp.Models;

namespace DinoApp.Services;

public interface IAuthService
{
    Task<bool> LoginAsync(string username, string password, bool rememberMe);
    void Logout();
    User? GetCurrentUser();
    bool IsAuthenticated();
    Task<bool> RegisterAsync(RegisterUserDto dto);
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
        Console.WriteLine($"=== AuthService инициализирован ===");
        Console.WriteLine($"Путь к файлу users.json: {_usersFilePath}");
        _users = LoadUsers();
        Console.WriteLine($"Загружено пользователей: {_users.Count}");
    }

    private List<User> LoadUsers()
    {
        if (File.Exists(_usersFilePath))
        {
            var json = File.ReadAllText(_usersFilePath);
            Console.WriteLine($"Загружен JSON: {json}");
            return JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();
        }

        Console.WriteLine("Файл users.json не найден, создаю администратора по умолчанию");

        // Создаём администратора по умолчанию при первом запуске
        var defaultUsers = new List<User>
        {
            new User
            {
                Id = 1,
                Username = "admin",
                PasswordHash = HashPassword("admin123"),
                Email = "admin@dino-mir.com",
                FullName = "Администратор",
                CreatedAt = DateTime.Now,
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
        Console.WriteLine($"Сохранено пользователей: {users.Count}");
        Console.WriteLine($"Сохранён JSON: {json}");
    }

    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }

    public async Task<bool> LoginAsync(string username, string password, bool rememberMe)
    {
        Console.WriteLine($"=== Попытка входа ===");
        Console.WriteLine($"Username: {username}");
        Console.WriteLine($"Password: {password}");

        var user = _users.FirstOrDefault(u => u.Username == username);

        if (user == null)
        {
            Console.WriteLine($"Пользователь {username} не найден");
            return false;
        }

        var hashedPassword = HashPassword(password);
        Console.WriteLine($"Хеш пароля: {hashedPassword}");
        Console.WriteLine($"Хеш в БД: {user.PasswordHash}");

        if (user.PasswordHash != hashedPassword)
        {
            Console.WriteLine($"Неверный пароль");
            return false;
        }

        var session = _httpContextAccessor.HttpContext?.Session;
        if (session != null)
        {
            session.SetString("UserId", user.Id.ToString());
            session.SetString("Username", user.Username);
            session.SetString("IsAdmin", user.IsAdmin.ToString());
            session.SetString("FullName", user.FullName ?? user.Username);

            Console.WriteLine($"Сессия создана для пользователя: {user.Username}");
        }

        return await Task.FromResult(true);
    }

    public void Logout()
    {
        _httpContextAccessor.HttpContext?.Session.Clear();
        Console.WriteLine("Пользователь вышел из системы");
    }

    public User? GetCurrentUser()
    {
        var userId = _httpContextAccessor.HttpContext?.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userId))
            return null;

        return _users.FirstOrDefault(u => u.Id.ToString() == userId);
    }

    public bool IsAuthenticated()
    {
        return !string.IsNullOrEmpty(_httpContextAccessor.HttpContext?.Session.GetString("UserId"));
    }

    public async Task<bool> RegisterAsync(RegisterUserDto dto)
    {
        Console.WriteLine($"=== Попытка регистрации ===");
        Console.WriteLine($"Username: {dto.Username}");
        Console.WriteLine($"Email: {dto.Email}");
        Console.WriteLine($"FullName: {dto.FullName}");

        // Проверка существования пользователя
        if (_users.Any(u => u.Username == dto.Username))
        {
            Console.WriteLine($"Пользователь с именем {dto.Username} уже существует");
            return await Task.FromResult(false);
        }

        if (_users.Any(u => u.Email == dto.Email))
        {
            Console.WriteLine($"Пользователь с email {dto.Email} уже существует");
            return await Task.FromResult(false);
        }

        var newUser = new User
        {
            Id = _users.Count > 0 ? _users.Max(u => u.Id) + 1 : 1,
            Username = dto.Username,
            PasswordHash = HashPassword(dto.Password),
            Email = dto.Email,
            FullName = dto.FullName,
            CreatedAt = DateTime.Now,
            IsAdmin = false
        };

        Console.WriteLine($"Создан новый пользователь с ID: {newUser.Id}");

        _users.Add(newUser);
        SaveUsers(_users);

        Console.WriteLine($"Регистрация успешна!");
        return await Task.FromResult(true);
    }
}