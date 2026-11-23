using System.ComponentModel.DataAnnotations;

namespace Vestigium.Models.ViewModels
{
    public class MaterialCreateVM
    {
        [Required]
        [Display(Name = "Número de Parte")]
        public string PartNumber { get; set; } =
            string.Empty;


        [Required]
        [Display(Name = "Descripción")]
        public string Description { get; set; } =
            string.Empty;


        [Required]
        [Display(Name = "Cantidad Inicial")]
        [Range(
            1,
            int.MaxValue,
            ErrorMessage =
                "La cantidad inicial debe ser mayor que cero.")]
        public int Quantity { get; set; }


        [Required]
        [Display(Name = "Ubicación inicial")]
        public string Rack { get; set; } =
            string.Empty;


        [Display(Name = "Código de Barras")]
        public string? Barcode { get; set; }


        public int Level { get; set; }
    }
}