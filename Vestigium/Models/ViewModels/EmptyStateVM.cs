namespace Vestigium.ViewModels
{
    public class EmptyStateVM
    {
        public string Title { get; set; } = "No data found";

        public string Description { get; set; }
            = "There is no information available.";

        public string Icon { get; set; }
            = "bi bi-inbox";

        public string? ButtonText { get; set; }

        public string? ButtonUrl { get; set; }

        public string ButtonClass { get; set; }
            = "btn btn-primary";
    }
}