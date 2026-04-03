using Microsoft.AspNetCore.Http;

namespace DinoApp.Models
{
    public class CreateDinosaurDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Clade { get; set; }
        public string? Era { get; set; }
        public string? Period { get; set; }
        public string? GroupName { get; set; }
        public string? Genus { get; set; }
        public string? Species { get; set; }
        public string? Description { get; set; }
        public IFormFile? PhotoFile { get; set; }
        public string? PhotoUrl { get; set; }
        public string? Size { get; set; }
        public string? FullDescription { get; set; }
        public string? Diet { get; set; }
        public string? Locomotion { get; set; }
        public string? Continent { get; set; }
        public string? Status { get; set; }
        public bool IsFeatured { get; set; }
        public bool AllowComments { get; set; }
        public string? DiscoveryLocation { get; set; }
        public string? Comments { get; set; }
    }
}