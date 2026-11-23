using Microsoft.AspNetCore.Identity;
namespace Vestigium.Data;
// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;

    public string Area { get; set; } = string.Empty;

    public bool MustChangePassword { get; set; } = false;
}
