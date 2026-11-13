using GestionCommerciale.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestionCommerciale.Infrastructure.Data.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Reference).IsRequired().HasMaxLength(50);
        builder.HasIndex(p => p.Reference).IsUnique();

        
        builder.Property(p => p.UnitPriceHT).HasColumnType("decimal(18,2)");
    }
}