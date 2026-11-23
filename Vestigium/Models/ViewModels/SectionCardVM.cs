namespace Vestigium.ViewModels
{
    public class SectionCardVM
    {
        public string Title { get; set; } = string.Empty;

        public string? Subtitle { get; set; }

        public string? Icon { get; set; }

        public string AreaClass { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;
    }
}