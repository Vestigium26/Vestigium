using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vestigium.Models;

namespace Vestigium.Data.Configurations
{
    public class MaterialConfiguration : IEntityTypeConfiguration<Material>
    {
        public void Configure(EntityTypeBuilder<Material> builder)
        {
            builder.ToTable("Materials");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.PartNumber)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.Description)
                .IsRequired()
                .HasMaxLength(250);

            builder.Property(x => x.Rack)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(x => x.Quantity)
                .IsRequired();

            builder.HasIndex(x => x.PartNumber);

            builder.HasIndex(x => x.Rack);
        }
    }
}