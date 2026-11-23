using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Vestigium.Data.Configurations;
using Vestigium.Models;
using Vestigium.Data;

namespace Vestigium.Data
{
    public class ApplicationDbContext
        : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Material> Materials { get; set; }

        public DbSet<Movement> Movements { get; set; }

        public DbSet<MaterialLocation> MaterialLocations { get; set; }

        public DbSet<Product> Products { get; set; }

        public DbSet<ProductBom> ProductBoms { get; set; }

        protected override void OnModelCreating(
            ModelBuilder builder)
        {

            base.OnModelCreating(builder);

            builder.Entity<ApplicationUser>()
                .Property(x => x.FullName)
                .HasMaxLength(150);

            builder.ApplyConfiguration(
                new MaterialConfiguration());

            builder.ApplyConfiguration(
                new MovementConfiguration());

            builder.ApplyConfiguration(
                new MaterialLocationConfiguration());

            builder.Entity<MaterialLocation>()
                .HasIndex(x => new
                {
                    x.MaterialId,
                    x.LocationCode
                })
                .IsUnique();

            builder.Entity<Product>()
                .HasIndex(p => p.ProductNumber)
                .IsUnique();

            builder.Entity<ProductBom>()
                .Property(pb => pb.Quantity)
                .HasPrecision(18, 4);

            builder.Entity<ProductBom>()
                .HasOne(pb => pb.Product)
                .WithMany(p => p.BomItems)
                .HasForeignKey(pb => pb.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ProductBom>()
                .HasOne(pb => pb.Material)
                .WithMany()
                .HasForeignKey(pb => pb.MaterialId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}