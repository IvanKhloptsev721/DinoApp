using Microsoft.AspNetCore.Identity;
using System.Text.Json;

namespace DinoApp.Services
{
    public class UserImportService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<UserImportService> _logger;

        public UserImportService(UserManager<IdentityUser> userManager, ILogger<UserImportService> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public async Task ImportUsersFromJsonAsync(string jsonFilePath)
        {
            try
            {
                if (!File.Exists(jsonFilePath))
                {
                    _logger.LogWarning($"JSON file not found: {jsonFilePath}");
                    return;
                }

                var json = await File.ReadAllTextAsync(jsonFilePath);
                var users = JsonSerializer.Deserialize<List<JsonUser>>(json);

                if (users == null) return;

                foreach (var jsonUser in users)
                {
                    var existingUser = await _userManager.FindByNameAsync(jsonUser.Username);
                    if (existingUser != null)
                    {
                        _logger.LogInformation($"User {jsonUser.Username} already exists, skipping...");
                        continue;
                    }

                    var identityUser = new IdentityUser
                    {
                        UserName = jsonUser.Username,
                        Email = jsonUser.Email,
                        EmailConfirmed = true
                    };

                    var password = string.IsNullOrEmpty(jsonUser.PasswordHash)
                        ? "TempPass123!"
                        : jsonUser.PasswordHash;

                    var result = await _userManager.CreateAsync(identityUser, password);

                    if (result.Succeeded)
                    {
                        _logger.LogInformation($"Created user: {jsonUser.Username}");

                        if (jsonUser.IsAdmin)
                        {
                            await _userManager.AddToRoleAsync(identityUser, "Admin");
                        }
                        else
                        {
                            await _userManager.AddToRoleAsync(identityUser, "User");
                        }
                    }
                    else
                    {
                        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                        _logger.LogError($"Failed to create user {jsonUser.Username}: {errors}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing users from JSON");
            }
        }

        private class JsonUser
        {
            public int Id { get; set; }
            public string Username { get; set; } = string.Empty;
            public string PasswordHash { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public bool IsAdmin { get; set; }
        }
    }
}