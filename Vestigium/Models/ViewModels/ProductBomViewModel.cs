namespace Vestigium.ViewModels
{
    public class ProductBomViewModel
    {
        public int ProductId { get; set; }

        public string ProductNumber { get; set; } = string.Empty;

        public string ProductDescription { get; set; } = string.Empty;

        public List<ProductBomItemViewModel> Items { get; set; }
            = new();
    }

    public class ProductBomItemViewModel
    {
        public int ProductBomId { get; set; }

        public int MaterialId { get; set; }

        public string MaterialNumber { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public decimal Quantity { get; set; }
    }
}