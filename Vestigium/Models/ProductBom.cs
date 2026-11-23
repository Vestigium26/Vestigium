namespace Vestigium.Models
{
    public class ProductBom
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        public int MaterialId { get; set; }

        public decimal Quantity { get; set; }

        public Product Product { get; set; } = null!;

        public Material Material { get; set; } = null!;
    }
}
