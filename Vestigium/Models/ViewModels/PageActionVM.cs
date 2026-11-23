namespace Vestigium.ViewModels
{
    public class PageActionVM
    {
        public string Text { get; set; } = string.Empty;

        public string Url { get; set; } = "#";

        public string Icon { get; set; } = "bi bi-circle";

        public string CssClass { get; set; }
            = "btn btn-primary";

        public bool NewTab { get; set; }
            = false;
    }
}