// Models/DinosaurDto.cs
namespace DinoApp.Models
{
    public class DinosaurDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Clade { get; set; }
        public string? Era { get; set; }
        public string? Period { get; set; }
        public string? GroupName { get; set; }
        public string? Genus { get; set; }
        public string? Species { get; set; }
        public string? Description { get; set; }

        // Вместо PhotoUrl храним путь к файлу
        public string? PhotoPath { get; set; }

        // Добавляем PhotoUrl для обратной совместимости
        public string? PhotoUrl
        {
            get => PhotoPath;
            set => PhotoPath = value;
        }

        public IFormFile? PhotoFile { get; set; }
        public string? Comments { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public string? Size { get; set; }
        public string? FullDescription { get; set; }
        public string? Diet { get; set; }
        public string? Locomotion { get; set; }
        public string? Continent { get; set; }
        public string? Status { get; set; }
        public bool IsFeatured { get; set; }
        public bool AllowComments { get; set; }
        public string? DiscoveryLocation { get; set; }
    }
}