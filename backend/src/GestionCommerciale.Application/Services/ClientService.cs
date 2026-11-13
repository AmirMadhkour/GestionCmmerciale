using GestionCommerciale.Application.Common.Exceptions;
using GestionCommerciale.Application.DTOs.Clients;
using GestionCommerciale.Application.Interfaces.Repositories;
using GestionCommerciale.Application.Mappings;
using GestionCommerciale.Domain.Entities;
using GestionCommerciale.Application.Interfaces.Services;

namespace GestionCommerciale.Application.Services;


public class ClientService : IClientService 
{
    private readonly IClientRepository _clientRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ClientService (IClientRepository clientRepository ,IUnitOfWork  unitOfWork)
    {
        _clientRepository = clientRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ClientDto> GetByIdAsync(Guid id)
    {
        var client = await _clientRepository.GetByIdAsync(id);
        if ( client == null )
            throw new NotFoundException($"Le cleint avec l'id  {id} n'a pas été trouvé");
        return client.ToDto();
    }

    public async Task<IReadOnlyList<ClientDto>> GetAllAsync()
    {
        var clients = await _clientRepository.GetAllAsync();
        return clients.Select(c => c.ToDto()).ToList();

    }

    public async Task<ClientDto> CreateClientAsync(CreateClientDto dto)
    {
        if (await _clientRepository.EmailExistsAsync(dto.Email))
            throw new ConflictException($"l'email {dto.Email} est utilisé");
        
        var client = Client.Create(dto.LastName , dto.FirstNameOrCompany , dto.Email , dto.Phone , dto.Address);
        await _clientRepository.AddAsync(client);
        await _unitOfWork.SaveChangesAsync();

        return client.ToDto();
    }

    public async Task UpdateClientAsync(Guid id ,UpdateClientDto dto)
    {
        var client = await _clientRepository.GetByIdAsync(id);
        if (client == null)
            throw new NotFoundException("Client introuvable");

        var newEmail = dto.Email.Trim().ToLower();
        if (newEmail != client.Email && await _clientRepository.EmailExistsAsync(newEmail))
            throw new ConflictException($"l'email {dto.Email} est utilisé");

        client.Update(dto.LastName , dto.FirstNameOrCompany,dto.Email ,dto.Phone, dto.Address);
         _clientRepository.Update(client);
        await  _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteClientAsync (Guid id)
    {
        var client = await _clientRepository.GetByIdAsync(id);
        if(client == null)
            throw new NotFoundException("Client introvable");

        if (await _clientRepository.HasOrdersAsync(id))
            throw new ConflictException("Impossible de supprimer un client qui a des commandes.");

        _clientRepository.Remove(client);
        await _unitOfWork.SaveChangesAsync();
    }   
}