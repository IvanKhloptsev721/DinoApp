using DinoApp.Models;

namespace DinoApp.Services
{
    public interface IAuthService
    {
        Task<(bool success, bool requiresTwoFactor, string? email)> LoginAsync(string username, string password, bool rememberMe);
        Task<bool> VerifyTwoFactorCodeAsync(string username, string code);
        Task<bool> RegisterAsync(RegisterDto model);
        Task LogoutAsync();
        void Logout();
        bool IsAuthenticated();

        Task<User?> GetCurrentUserAsync();
        User? GetCurrentUser();
        Task<User?> GetUserByIdAsync(string id);
        User? GetUserById(int id);
        Task<List<User>> GetAllUsersAsync();
        List<User> GetAllUsers();

        Task<bool> IsAdminAsync();
        bool IsAdmin();

        Task<bool> IsDeveloperAsync();
        bool IsDeveloper();

        Task<bool> UpdateProfileAsync(string userId, UpdateProfileDto model);
        Task<bool> UpdateProfileAsync(int userId, UpdateProfileDto model);
        Task<bool> ToggleAdminAsync(string userId);
        Task<bool> ToggleDeveloperAsync(string userId);
        Task<bool> ToggleAdminByDeveloperAsync(string userId);
    }
}