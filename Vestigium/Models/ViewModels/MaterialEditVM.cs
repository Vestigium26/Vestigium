using System.ComponentModel.DataAnnotations;

namespace Vestigium.Models.ViewModels
{
    public class MaterialEditVM
    {
        public int Id { get; set; }


        [Required]
        [Display(Name = "Número de Parte")]
        public string PartNumber { get; set; } =
            string.Empty;


        [Required]
        [Display(Name = "Descripción")]
        public string Description { get; set; } =
            string.Empty;


        public int Quantity { get; set; }


        public string Rack { get; set; } =
            string.Empty;


        public string? Barcode { get; set; }
    }
}