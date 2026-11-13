using GestionCommerciale.Domain.Entities;

namespace GestionCommerciale.Application.Interfaces.Security;

public interface  IPasswordHasher
{
    string HashPassword(string password);

    bool VerifyPassword (string password, string hashedPassword);
}