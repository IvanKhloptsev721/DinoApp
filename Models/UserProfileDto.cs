using System.ComponentModel.DataAnnotations;

namespace DinoApp.Models
{
    public class LoginDto
    {
        [Required(ErrorMessage = "Введите имя пользователя")]
        [Display(Name = "Имя пользователя")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите пароль")]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Запомнить меня")]
        public bool RememberMe { get; set; }
    }


    public class RegisterDto
    {
        [Required(ErrorMessage = "Введите имя пользователя")]
        [Display(Name = "Имя пользователя")]
        [StringLength(50, MinimumLength = 3)]
        [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Только буквы, цифры и _")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите email")]
        [EmailAddress(ErrorMessage = "Некорректный email")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите пароль")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6)]
        [Display(Name = "Пароль")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Подтвердите пароль")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Пароли не совпадают")]
        [Display(Name = "Подтверждение пароля")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Display(Name = "Полное имя")]
        [StringLength(100)]
        public string? FullName { get; set; }
        }
    

    public class UpdateProfileDto
    {
        [Display(Name = "Полное имя")]
        [StringLength(100)]
        public string? FullName { get; set; }

        [Display(Name = "О себе")]
        [StringLength(500)]
        public string? Bio { get; set; }

        [Display(Name = "Email")]
        [EmailAddress]
        public string? Email { get; set; }

        public string? AvatarUrl { get; set; }
    }

    public class UserProfileDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public string? Bio { get; set; }
        public string? AvatarUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public bool IsAdmin { get; set; }
        public int DinosaursCount { get; set; }
        public int CommentsCount { get; set; }
    }
}