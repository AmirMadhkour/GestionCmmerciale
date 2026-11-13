using GestionCommerciale.Application.Interfaces.Repositories;
using GestionCommerciale.Domain.Entities;
using GestionCommerciale.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestionCommerciale.Infrastructure.Repositories; 

public class ClientRepository : Repository<Client> , IClientRepository
{
    public ClientRepository(AppDbContext context) : base(context) {}

    public async Task<bool> EmailExistsAsync(string email )
    {
        return await _dbSet.AnyAsync(c=>c.Email == email);
    }
    public async Task<bool> HasOrdersAsync (Guid ClientId)
    {
        return await _context.Orders.AnyAsync(o=>o.ClientId == ClientId);
    }
}