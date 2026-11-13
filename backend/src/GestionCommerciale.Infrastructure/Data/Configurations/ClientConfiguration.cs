using GestionCommerciale.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestionCommerciale.Infrastructure.Data;

public class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.ToTable("Clients");
        builder.HasKey(c=>c.Id);

        builder.Property(c=>c.Email).IsRequired().HasMaxLength(150);
        builder.HasIndex(c=>c.Email).IsUnique();

        builder.Property(c=>c.Address)
            .HasConversion(
                v=>string.Join(";",v),
                v=> v.Split(';' , StringSplitOptions.RemoveEmptyEntries),
                new ValueComparer<string[]>(
                    (a, b) => a!.SequenceEqual(b!),
                    a => a.Aggregate(0, (h, s) => HashCode.Combine(h, s.GetHashCode())),
                    a => a.ToArray())
            );

        builder.HasMany(c=>c.Orders)
                .WithOne(o=>o.Client)
                .HasForeignKey(o=>o.ClientId)
                .OnDelete(DeleteBehavior.Restrict);
        builder.Navigation(c=>c.Orders)
            .UsePropertyAccessMode(PropertyAccessMode.Field).HasField("_orders");
    }
}