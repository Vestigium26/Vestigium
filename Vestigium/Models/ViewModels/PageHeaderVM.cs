namespace Vestigium.ViewModels
{
    public class PageHeaderVM
    {
        public string Title { get; set; } = string.Empty;

        public string? Subtitle { get; set; }

        public string? Icon { get; set; }

        public string? ActionText { get; set; }

        public string? ActionUrl { get; set; }

        public string ActionClass { get; set; } = "btn btn-primary";

        public string? ActionIcon { get; set; }
    }
}