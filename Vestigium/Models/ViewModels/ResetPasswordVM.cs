using System.ComponentModel.DataAnnotations;

public class ResetPasswordVM
{
    public string UserId { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña temporal es obligatoria.")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña Temporal")]
    [StringLength(
        100,
        MinimumLength = 8,
        ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
    [RegularExpression(
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).+$",
        ErrorMessage = "La contraseña debe contener mayúsculas, minúsculas, números y caracteres especiales.")]
    public string TemporaryPassword { get; set; } = string.Empty;

    [Display(Name = "Obligar cambio al iniciar sesión")]
    public bool ForceChange { get; set; } = true;

    public string FullName { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;
}