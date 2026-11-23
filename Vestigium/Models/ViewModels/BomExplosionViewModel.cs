namespace Vestigium.Models.ViewModels
{
    public class BomExplosionViewModel
    {
        public int ProductId { get; set; }

        public string ProductNumber { get; set; } = string.Empty;

        public string ProductDescription { get; set; } = string.Empty;

        public decimal BuildQuantity { get; set; }

        public List<BomExplosionItemViewModel> Materials { get; set; }
            = new();
    }

    public class BomExplosionItemViewModel
    {
        public string PartNumber { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public decimal BomQuantity { get; set; }

        public decimal RequiredQuantity { get; set; }

        public decimal AvailableQuantity { get; set; }

        public decimal ShortageQuantity { get; set; }
    }
}