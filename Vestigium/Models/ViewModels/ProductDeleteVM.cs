namespace Vestigium.Models.ViewModels
{
    public class ProductDeleteVM
    {
        public int Id { get; set; }

        public string ProductNumber { get; set; } = string.Empty;

        public string ProductDescription { get; set; } = string.Empty;

        public int BomCount { get; set; }

        public string Password { get; set; } = string.Empty;
    }
}
