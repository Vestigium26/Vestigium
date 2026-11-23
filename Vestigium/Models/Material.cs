using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vestigium.Models
{
    public class Material
    {
        public int Id { get; set; }


        [Required]
        [StringLength(100)]
        [Display(Name = "Número de Parte")]
        public string PartNumber { get; set; } =
            string.Empty;


        [Required]
        [StringLength(250)]
        [Display(Name = "Descripción")]
        public string Description { get; set; } =
            string.Empty;


        [Display(Name = "Cantidad")]
        public int Quantity { get; set; }


        [Required]
        [StringLength(20)]
        [Display(Name = "Rack Asignado")]
        public string Rack { get; set; } =
            string.Empty;


        [StringLength(100)]
        [Display(Name = "Código de Barras")]
        public string? Barcode { get; set; } =
            string.Empty;


        [NotMapped]
        public string? QrContent { get; set; }


        public ICollection<MaterialLocation> Locations { get; set; }
            = new List<MaterialLocation>();
    }
}