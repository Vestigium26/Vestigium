namespace Vestigium.Models.ViewModels
{
    public class MaterialInventoryItemVM
    {
        public int Id { get; set; }

        public string PartNumber { get; set; } =
            string.Empty;

        public string Description { get; set; } =
            string.Empty;

        public int Quantity { get; set; }

        public string Rack { get; set; } =
            string.Empty;

        public List<string> UsedInProducts { get; set; } =
            new();
    }
}