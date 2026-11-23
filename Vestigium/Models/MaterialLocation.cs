using System.ComponentModel.DataAnnotations;

namespace Vestigium.Models
{
    public class MaterialLocation
    {
        public int Id { get; set; }

        [Required]
        public int MaterialId { get; set; }

        public Material Material { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string LocationCode { get; set; } = string.Empty;

        public int Quantity { get; set; }
    }
}