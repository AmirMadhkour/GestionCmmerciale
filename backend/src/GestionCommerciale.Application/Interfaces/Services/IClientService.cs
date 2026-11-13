using GestionCommerciale.Application.DTOs.Clients;

namespace GestionCommerciale.Application.Interfaces.Services;

public interface IClientService
{
    Task<ClientDto> GetByIdAsync(Guid id);
    Task<ClientDto> CreateClientAsync (CreateClientDto dto);
    Task UpdateClientAsync (Guid id , UpdateClientDto dto);
    Task DeleteClientAsync ( Guid id);
    Task<IReadOnlyList<ClientDto>> GetAllAsync ();

}