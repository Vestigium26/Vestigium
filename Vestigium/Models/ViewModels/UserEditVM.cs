using System.ComponentModel.DataAnnotations;

namespace Vestigium.Models.ViewModels
{
    public class UserEditVM
    {
        public string Id { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Número de Empleado")]
        [RegularExpression(@"^\d+$",
        ErrorMessage = "Solo se permiten números.")]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Nombre Completo")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = string.Empty;

        [Required]
        public string Area { get; set; } = string.Empty;
    }
}