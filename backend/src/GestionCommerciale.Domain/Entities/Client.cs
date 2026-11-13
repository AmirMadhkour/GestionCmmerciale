using GestionCommerciale.Domain.Common;
using GestionCommerciale.Domain.Enums;

namespace GestionCommerciale.Domain.Entities;

public class Client : BaseEntity
{
    private Client ( ) {}
    public string LastName {get; set;} = string.Empty ;

    public string FirstNameOrCompany {get; set;} = string.Empty ;

    public string Phone {get; set;} = string.Empty;

    public string Email {get; set;} = string.Empty;

    public string[] Address {get; set;} = Array.Empty<string>();

    private readonly List<Order> _orders=new();
    public IReadOnlyCollection<Order> Orders => _orders.AsReadOnly();

  
    public static Client Create(string lastName, string firstNameOrCompany, string email, string phone, string[] address)
    {
        
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("L'email est obligatoire.");

        return new Client
        {
            Id = Guid.NewGuid(),
            LastName = lastName?.Trim() ?? string.Empty,
            FirstNameOrCompany = firstNameOrCompany?.Trim() ?? string.Empty,
            Email = email.Trim().ToLower(), 
            Phone = phone?.Trim() ?? string.Empty,
            Address = address
        };
    }

   
    public void Update(string lastName, string firstNameOrCompany, string email, string phone, string[] address)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("L'email est obligatoire.");

        LastName = lastName?.Trim() ?? string.Empty;
        FirstNameOrCompany = firstNameOrCompany?.Trim() ?? string.Empty;
        Email = email.Trim().ToLower();
        Phone = phone?.Trim() ?? string.Empty;
        Address = address;
    }
}