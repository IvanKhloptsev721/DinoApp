namespace DinoApp.Models
{
    public class Page
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? MetaDescription { get; set; }
        public string? MetaKeywords { get; set; }
        public bool IsPublished { get; set; }
        public string? ButtonText { get; set; }
        public string? ButtonUrl { get; set; }
        public string? ButtonColor { get; set; }
        public bool ShowInNavigation { get; set; }
        public int NavigationOrder { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreatePageDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Slug { get; set; }
        public string Content { get; set; } = string.Empty;
        public string? MetaDescription { get; set; }
        public string? MetaKeywords { get; set; }
        public bool IsPublished { get; set; } = true;
        public string? ButtonText { get; set; }
        public string? ButtonUrl { get; set; }
        public string? ButtonColor { get; set; }
        public bool ShowInNavigation { get; set; }
        public int NavigationOrder { get; set; }
    }

    public class UpdatePageDto
    {
        public string? Title { get; set; }
        public string? Slug { get; set; }
        public string? Content { get; set; }
        public string? MetaDescription { get; set; }
        public string? MetaKeywords { get; set; }
        public bool? IsPublished { get; set; }
        public string? ButtonText { get; set; }
        public string? ButtonUrl { get; set; }
        public string? ButtonColor { get; set; }
        public bool? ShowInNavigation { get; set; }
        public int? NavigationOrder { get; set; }
    }
}