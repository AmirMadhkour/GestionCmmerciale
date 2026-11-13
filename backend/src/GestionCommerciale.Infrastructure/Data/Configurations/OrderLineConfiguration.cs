using GestionCommerciale.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestionCommerciale.Infrastructure.Data.Configurations;

public class OrderLineConfiguration : IEntityTypeConfiguration<OrderLine>
{
    public  void Configure (EntityTypeBuilder<OrderLine> builder)
    {
        builder.ToTable("OrderLines");
        builder.HasKey(ol=>ol.Id);

        builder.Property(ol => ol.UnitPrice).HasColumnType("decimal(18,2)");
        builder.Property(ol => ol.LineTotal).HasColumnType("decimal(18,2)");

        builder.HasOne(ol=>ol.Product)
                .WithMany()
                .HasForeignKey(ol=>ol.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
    }
}