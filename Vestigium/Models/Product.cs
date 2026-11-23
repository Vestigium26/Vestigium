namespace Vestigium.Models
{
    public class Product
    {
        public int Id { get; set; }

        public string ProductNumber { get; set; } = string.Empty;

        public string ProductDescription { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public ICollection<ProductBom> BomItems { get; set; }
            = new List<ProductBom>();
    }
}