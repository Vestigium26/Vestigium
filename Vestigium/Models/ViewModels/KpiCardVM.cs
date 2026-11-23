namespace Vestigium.ViewModels
{
    public class KpiCardVM
    {
        public string Title { get; set; } = string.Empty;

        public string Value { get; set; } = "0";

        public string? Subtitle { get; set; }

        public string? Icon { get; set; }

        public string ColorClass { get; set; } = "kpi-primary";

        public string? Url { get; set; } // Nuevo
    }
}