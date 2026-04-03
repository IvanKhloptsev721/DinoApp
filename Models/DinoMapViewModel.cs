namespace DinoApp.Models
{
    public class DinoMapViewModel
    {
        public List<DinosaurDto> Dinosaurs { get; set; } = new();
        public string? SelectedContinent { get; set; }
        public string? SelectedDinoName { get; set; }
    }
}