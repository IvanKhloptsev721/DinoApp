using System.ComponentModel.DataAnnotations;

namespace DinoApp.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Пожалуйста, укажите ваше имя")]
        [Display(Name = "Имя")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Имя должно содержать от 2 до 50 символов")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Пожалуйста, укажите ваш возраст")]
        [Display(Name = "Возраст")]
        [Range(1, 120, ErrorMessage = "Возраст должен быть от 1 до 120 лет")]
        public int Age { get; set; }

        [Display(Name = "О себе")]
        [StringLength(500, ErrorMessage = "Рассказ о себе не должен превышать 500 символов")]
        public string Bio { get; set; } = string.Empty;
    }
}