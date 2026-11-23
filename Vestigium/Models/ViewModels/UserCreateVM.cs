using System.ComponentModel.DataAnnotations;

namespace Vestigium.Models.ViewModels
{
    public class UserCreateVM
    {
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
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Rol")]
        public string Role { get; set; } = "User";

        [Required]
        [Display(Name = "Área")]
        public string Area { get; set; } = string.Empty;
    }
}