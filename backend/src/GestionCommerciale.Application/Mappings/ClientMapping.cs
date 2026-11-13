using GestionCommerciale.Application.DTOs.Clients;
using GestionCommerciale.Domain.Entities;

namespace GestionCommerciale.Application.Mappings;

public static class ClientMapping
{
    public static ClientDto ToDto (this Client client)
    {
        return new ClientDto
        {
            Id = client.Id ,
            LastName = client.LastName ,
            FirstNameOrCompany = client.FirstNameOrCompany,
            Email = client.Email,
            Phone = client.Phone,
            Address = client.Address,
            CreatedAt  = client.CreatedAt
        };
    }
}