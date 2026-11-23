using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class VestigiumContext(DbContextOptions<VestigiumContext> options) : IdentityDbContext<Vestigium.Data.ApplicationUser>(options)
{
}
