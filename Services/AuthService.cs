using Microsoft.AspNetCore.Identity;
using DinoApp.Models;
using System.Security.Claims;

namespace DinoApp.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IHttpContextAccessor httpContextAccessor,
            ILogger<AuthService> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public bool IsAuthenticated()
        {
            return _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
        }

        public async Task<(bool success, bool requiresTwoFactor, string? email)> LoginAsync(string username, string password, bool rememberMe)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
                return (false, false, null);

            var result = await _signInManager.PasswordSignInAsync(user, password, rememberMe, false);

            if (result.Succeeded)
            {
                return (true, false, null);
            }

            return (false, false, null);
        }

        public async Task<bool> VerifyTwoFactorCodeAsync(string username, string code)
        {
            return false; // Временно отключено
        }

        public async Task<bool> RegisterAsync(RegisterDto model)
        {
            var user = new IdentityUser
            {
                UserName = model.Username,
                Email = model.Email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "User");
                return true;
            }

            return false;
        }

        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        public void Logout()
        {
            LogoutAsync().Wait();
        }

        public async Task<User?> GetCurrentUserAsync()
        {
            var identityUser = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext?.User);
            if (identityUser == null)
                return null;

            return await ConvertToUser(identityUser);
        }

        public User? GetCurrentUser()
        {
            return GetCurrentUserAsync().Result;
        }

        public async Task<User?> GetUserByIdAsync(string id)
        {
            var identityUser = await _userManager.FindByIdAsync(id);
            if (identityUser == null)
                return null;

            return await ConvertToUser(identityUser);
        }

        public User? GetUserById(int id)
        {
            return GetUserByIdAsync(id.ToString()).Result;
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            var identityUsers = _userManager.Users.ToList();
            var users = new List<User>();

            foreach (var identityUser in identityUsers)
            {
                users.Add(await ConvertToUser(identityUser));
            }

            return users;
        }

        public List<User> GetAllUsers()
        {
            return GetAllUsersAsync().Result;
        }

        public async Task<bool> IsAdminAsync()
        {
            var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext?.User);
            if (user == null)
                return false;

            return await _userManager.IsInRoleAsync(user, "Admin");
        }

        public bool IsAdmin()
        {
            return IsAdminAsync().Result;
        }

        public async Task<bool> IsDeveloperAsync()
        {
            var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext?.User);
            if (user == null)
                return false;

            return await _userManager.IsInRoleAsync(user, "Developer");
        }

        public bool IsDeveloper()
        {
            return IsDeveloperAsync().Result;
        }

        public async Task<bool> UpdateProfileAsync(string userId, UpdateProfileDto model)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return false;

            if (!string.IsNullOrEmpty(model.Email) && model.Email != user.Email)
            {
                user.Email = model.Email;
            }

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                _logger.LogInformation($"Profile updated for user {userId}");
                // Здесь можно сохранить дополнительные данные в отдельную таблицу
                return true;
            }

            return false;
        }

        public async Task<bool> UpdateProfileAsync(int userId, UpdateProfileDto model)
        {
            return await UpdateProfileAsync(userId.ToString(), model);
        }

        public async Task<bool> ToggleAdminAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return false;

            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

            if (isAdmin)
            {
                await _userManager.RemoveFromRoleAsync(user, "Admin");
            }
            else
            {
                await _userManager.AddToRoleAsync(user, "Admin");
            }

            return true;
        }

        public async Task<bool> ToggleDeveloperAsync(string userId)
        {
            var currentUser = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext?.User);
            if (currentUser == null)
                return false;

            // Только Developer может назначать/снимать Developer
            if (!await _userManager.IsInRoleAsync(currentUser, "Developer"))
                return false;

            var targetUser = await _userManager.FindByIdAsync(userId);
            if (targetUser == null)
                return false;

            var isDeveloper = await _userManager.IsInRoleAsync(targetUser, "Developer");

            if (isDeveloper)
            {
                await _userManager.RemoveFromRoleAsync(targetUser, "Developer");
            }
            else
            {
                await _userManager.AddToRoleAsync(targetUser, "Developer");
            }

            return true;
        }

        public async Task<bool> ToggleAdminByDeveloperAsync(string userId)
        {
            var currentUser = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext?.User);
            if (currentUser == null)
                return false;

            // Developer или Admin могут назначать/снимать админов
            if (!await _userManager.IsInRoleAsync(currentUser, "Developer") &&
                !await _userManager.IsInRoleAsync(currentUser, "Admin"))
                return false;

            var targetUser = await _userManager.FindByIdAsync(userId);
            if (targetUser == null)
                return false;

            var isAdmin = await _userManager.IsInRoleAsync(targetUser, "Admin");

            if (isAdmin)
            {
                await _userManager.RemoveFromRoleAsync(targetUser, "Admin");
            }
            else
            {
                await _userManager.AddToRoleAsync(targetUser, "Admin");
            }

            return true;
        }

        private async Task<User> ConvertToUser(IdentityUser identityUser)
        {
            var isAdmin = await _userManager.IsInRoleAsync(identityUser, "Admin");
            var isDeveloper = await _userManager.IsInRoleAsync(identityUser, "Developer");

            return new User
            {
                Id = identityUser.Id,
                UserName = identityUser.UserName ?? string.Empty,
                Email = identityUser.Email ?? string.Empty,
                CreatedAt = DateTime.Now,
                IsAdmin = isAdmin || isDeveloper,
                DinosaursCreated = 0,
                CommentsCount = 0
            };
        }
    }
}