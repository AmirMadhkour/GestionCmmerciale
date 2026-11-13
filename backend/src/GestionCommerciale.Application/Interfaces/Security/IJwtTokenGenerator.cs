using GestionCommerciale.Domain.Entities;

namespace GestionCommerciale.Application.Interfaces.Security;

public interface  IJwtTokenGenerator
{
    string GenerateToken(User user);
}