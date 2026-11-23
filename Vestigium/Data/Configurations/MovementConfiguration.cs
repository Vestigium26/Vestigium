using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vestigium.Models;

namespace Vestigium.Data.Configurations
{
    public class MovementConfiguration
        : IEntityTypeConfiguration<Movement>
    {
        public void Configure(
            EntityTypeBuilder<Movement> builder)
        {
            builder.ToTable("Movements");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.UserName)
                .HasMaxLength(100);

            builder.Property(x => x.ReportNumber)
                .HasMaxLength(100);

            builder.Property(x => x.Comments)
                .HasMaxLength(500);

            builder.HasIndex(x => x.MovementDate);

            builder.HasIndex(x => x.MaterialId);

            builder.HasOne(x => x.Material)
                .WithMany()
                .HasForeignKey(x => x.MaterialId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}