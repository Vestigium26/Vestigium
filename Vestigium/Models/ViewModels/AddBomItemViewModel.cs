using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Vestigium.Models.ViewModels
{
    public class AddBomItemViewModel
    {
        public int ProductId { get; set; }

        [Required]
        public int MaterialId { get; set; }

        [Range(0.0001, double.MaxValue,
            ErrorMessage = "La cantidad debe ser mayor que cero.")]
        public decimal? Quantity { get; set; }

        public List<SelectListItem> Materials { get; set; } = new();

        public string SearchTerm { get; set; } = string.Empty;

        // New

        public string? Password { get; set; }

        public bool RequiresPassword { get; set; }

        public string SelectedMaterialNumber { get; set; } = string.Empty;

        public string SelectedMaterialDescription { get; set; } = string.Empty;
    }
}