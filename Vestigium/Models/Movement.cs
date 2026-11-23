using System.ComponentModel.DataAnnotations;
using Vestigium.Models.Enums;

namespace Vestigium.Models
{
    public class Movement
    {
        public int Id { get; set; }

        public DateTime MovementDate { get; set; }

        public string UserId { get; set; } = string.Empty;

        [Required]
        public string UserName { get; set; } = string.Empty;

        public int MaterialId { get; set; }

        public Material Material { get; set; } = null!;

        public MovementType MovementType { get; set; }

        [Required]
        [StringLength(20)]
        public string ReportNumber { get; set; } = string.Empty;

        public int Quantity { get; set; }

        [Required]
        public string OriginLocation { get; set; } = string.Empty;

        [Required]
        public string DestinationLocation { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Comments { get; set; }

        // New cambios usuario en registro de movimientos

        public string? ReceivedByUserId { get; set; }

        public string? ReceivedByName { get; set; }

        public string? ReceivedByEmployeeNumber { get; set; }

        public string? RegisteredByName { get; set; }
    }
}