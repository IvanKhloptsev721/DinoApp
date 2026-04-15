using Microsoft.AspNetCore.Identity;

namespace DinoApp.Models
{
    public class User
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;  // ← Было Username
        public string Email { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? Bio { get; set; }
        public string? AvatarUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public bool IsAdmin { get; set; }

        // Для совместимости со старым кодом (если где-то используется Username)
        public string Username
        {
            get => UserName;
            set => UserName = value;
        }

        // Статистика (опционально)
        public int DinosaursCreated { get; set; }
        public int CommentsCount { get; set; }
    }
}