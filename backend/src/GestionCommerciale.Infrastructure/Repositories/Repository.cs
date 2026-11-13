using GestionCommerciale.Application.Interfaces.Repositories;
using GestionCommerciale.Domain.Common;
using GestionCommerciale.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestionCommerciale.Infrastructure.Repositories; 

public class Repository<T> : IRepository<T> where T :BaseEntity
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<T> _dbSet ;
    public Repository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(Guid id)
    {
        return await _dbSet.FindAsync(id);
    }

    public virtual async Task<IReadOnlyList<T>> GetAllAsync()
    {
        return await _dbSet.AsNoTracking().ToListAsync();
    }
    public async Task AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
    }

    public void Update (T entity)
    {
        _dbSet.Update(entity);
    }
    public void Remove (T entity)
    {
        _dbSet.Remove(entity);
    }
}