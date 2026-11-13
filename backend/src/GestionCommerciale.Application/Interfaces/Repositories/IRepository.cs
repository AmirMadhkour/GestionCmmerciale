using GestionCommerciale.Domain.Common;

namespace GestionCommerciale.Application.Interfaces.Repositories;

public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<T>> GetAllAsync();
    Task AddAsync(T entity);
    void Remove(T entity);
    void Update(T entity);
}