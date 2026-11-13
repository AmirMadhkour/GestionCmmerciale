using GestionCommerciale.Application.Interfaces.Repositories;
using GestionCommerciale.Infrastructure.Data;

namespace GestionCommerciale.Infrastructure.Repositories; 

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context ;
    public UnitOfWork (  AppDbContext context  )
    {
        _context = context ;
    }
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}