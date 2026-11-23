using System.ComponentModel.DataAnnotations;
using Vestigium.Models.Enums;

namespace Vestigium.Models.ViewModels
{
    public class MovementCreateVM
    {
        [Range(
            1,
            int.MaxValue,
            ErrorMessage = "Seleccione un material.")]
        public int MaterialId { get; set; }


        public string Description { get; set; }
            = string.Empty;


        [EnumDataType(
            typeof(MovementType),
            ErrorMessage = "Seleccione un tipo de movimiento válido.")]
        public MovementType MovementType { get; set; }


        [Range(
            1,
            int.MaxValue,
            ErrorMessage = "La cantidad debe ser mayor que cero.")]
        [Display(Name = "Cantidad")]
        public int Quantity { get; set; }


        [Required(
            ErrorMessage = "Seleccione una ubicación.")]
        [Display(Name = "Ubicación")]
        public string OriginLocation { get; set; }
            = string.Empty;


        public string DestinationLocation { get; set; }
            = string.Empty;


        public string? Comments { get; set; }


        [Required(
            ErrorMessage = "Seleccione la persona que recibe el material.")]
        [Display(Name = "Recibido por")]
        public string? ReceivedByUserId { get; set; }


        public string? ReceivedByEmployeeNumber { get; set; }
    }
}