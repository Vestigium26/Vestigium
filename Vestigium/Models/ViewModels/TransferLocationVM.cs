using System.ComponentModel.DataAnnotations;

namespace Vestigium.Models.ViewModels
{
    public class TransferLocationVM
    {
        public int MaterialId { get; set; }

        public string PartNumber { get; set; }
            = string.Empty;


        [Required(
            ErrorMessage = "Seleccione una ubicación origen.")]
        [Display(Name = "Ubicación Origen")]
        public string OriginLocation { get; set; }
            = string.Empty;


        /*
         * Se construye en el Controller utilizando:
         * DestinationRack + DestinationLevel.
         */
        public string DestinationLocation { get; set; }
            = string.Empty;


        [Required(
            ErrorMessage = "Seleccione el rack destino.")]
        [Display(Name = "Rack Destino")]
        public string? DestinationRack { get; set; }


        [Required(
            ErrorMessage = "Seleccione el nivel destino.")]
        [Display(Name = "Nivel Destino")]
        public int? DestinationLevel { get; set; }


        [Required]
        [Range(
            1,
            int.MaxValue,
            ErrorMessage = "La cantidad debe ser mayor que cero.")]
        [Display(Name = "Cantidad")]
        public int Quantity { get; set; }
    }
}