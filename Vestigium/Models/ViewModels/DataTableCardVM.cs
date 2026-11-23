namespace Vestigium.ViewModels
{
    public class DataTableCardVM
    {
        public string Title { get; set; } = string.Empty;

        public string? Icon { get; set; }

        public string? HeaderContent { get; set; }

        public string? ToolbarContent { get; set; }

        public string TableContent { get; set; } = string.Empty;

        public string? FooterContent { get; set; }
    }
}