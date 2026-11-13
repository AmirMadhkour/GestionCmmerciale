namespace GestionCommerciale.Application.DTOs.Clients;

public class CreateClientDto
{
   
    public string LastName {get; set;} = string.Empty;
    public string FirstNameOrCompany {get; set;} = string.Empty;
    public string Email {get; set;} = string.Empty;
    public string Phone {get; set;} = string.Empty;
    public string [] Address {get; set;} = Array.Empty<string>();
    
}

