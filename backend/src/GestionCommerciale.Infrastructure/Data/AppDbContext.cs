using GestionCommerciale.Domain.Common;
using GestionCommerciale.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace GestionCommerciale.Infrastructure.Data ;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

    public DbSet<User> Users => Set<User>();
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderLine> OrderLines => Set<OrderLine>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Les Id (Guid) sont générés par le domaine : EF ne doit pas les générer.
        // Sinon une nouvelle ligne ajoutée à une commande déjà suivie est traitée comme "Modified"
        // et SaveChanges lève une DbUpdateConcurrencyException.
        foreach (var entityType in modelBuilder.Model.GetEntityTypes()
                     .Where(t => typeof(BaseEntity).IsAssignableFrom(t.ClrType)))
        {
            modelBuilder.Entity(entityType.ClrType)
                .Property(nameof(BaseEntity.Id))
                .ValueGeneratedNever();
        }
    }
} 