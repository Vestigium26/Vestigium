namespace Vestigium.ViewModels
{
    public class StatusBadgeVM
    {
        public string Text { get; set; } = string.Empty;

        public string Type { get; set; } = "neutral";

        public bool ShowIcon { get; set; } = true;
    }
}