using GestionCommerciale.Application.DTOs.Clients;
using GestionCommerciale.Domain.Entities;

namespace GestionCommerciale.Application.Interfaces.Repositories;

public interface IClientRepository : IRepository<Client>
{
    Task<bool> EmailExistsAsync(string email);
    Task<bool> HasOrdersAsync(Guid clientId);

}