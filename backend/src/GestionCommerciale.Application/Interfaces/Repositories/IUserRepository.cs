using GestionCommerciale.Domain.Entities;

namespace GestionCommerciale.Application.Interfaces.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync (string email);
    Task<bool> EmailExistAsync (string email);
}