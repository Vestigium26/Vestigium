using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vestigium.Models;

namespace Vestigium.Data.Configurations
{
    public class MaterialLocationConfiguration
        : IEntityTypeConfiguration<MaterialLocation>
    {
        public void Configure(
            EntityTypeBuilder<MaterialLocation> builder)
        {
            builder.ToTable("MaterialLocations");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.LocationCode)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.Quantity)
                .IsRequired();

            builder.HasOne(x => x.Material)
                .WithMany(x => x.Locations)
                .HasForeignKey(x => x.MaterialId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x =>
                new
                {
                    x.MaterialId,
                    x.LocationCode
                })
                .IsUnique();
        }
    }
}