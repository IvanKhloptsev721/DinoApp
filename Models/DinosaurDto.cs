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
        public string? PhotoUrl { get; set; }
        public string? Comments { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
