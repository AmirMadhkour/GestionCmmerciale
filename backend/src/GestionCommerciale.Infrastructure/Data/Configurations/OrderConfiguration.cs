using GestionCommerciale.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestionCommerciale.Infrastructure.Data.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");
        builder.HasKey(o => o.Id);

        // Numéro de commande unique (EF ajoute le filtre IS NOT NULL pour SQL Server)
        builder.HasIndex(o => o.OrderNumber).IsUnique();

        
        builder.Property(o => o.TotalHt).HasColumnType("decimal(18,2)");
        builder.Property(o => o.TotalTtc).HasColumnType("decimal(18,2)");
        builder.Property(o => o.OrderStatus).HasConversion<string>();

       
        builder.HasMany(o => o.Lines)
               .WithOne(l => l.Order)
               .HasForeignKey(l => l.OrderId)
               .OnDelete(DeleteBehavior.Cascade); 

        builder.Navigation(o => o.Lines)
               .UsePropertyAccessMode(PropertyAccessMode.Field)
               .HasField("_lines");
    }
}