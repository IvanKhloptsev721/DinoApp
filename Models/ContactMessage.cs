using System.ComponentModel.DataAnnotations;

namespace DinoApp.Models
{
    public class ContactMessage
    {
        [Required(ErrorMessage = "Введите имя")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите email")]
        [EmailAddress(ErrorMessage = "Некорректный email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите тему")]
        public string Subject { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите сообщение")]
        public string Message { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}