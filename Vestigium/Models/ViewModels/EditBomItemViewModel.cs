namespace Vestigium.Models.ViewModels
{
    public class EditBomItemViewModel
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        public int MaterialId { get; set; }


        // Material

        public string MaterialNumber { get; set; } =
            string.Empty;

        public string MaterialDescription { get; set; } =
            string.Empty;


        // Edicion

        public decimal Quantity { get; set; }

        public string? Password { get; set; }
    }
}