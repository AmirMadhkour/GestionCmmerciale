using GestionCommerciale.Application.DTOs.Auth;
using GestionCommerciale.Domain.Entities;

namespace GestionCommerciale.Application.Mappings;

public static class  UserMapper {
    public static CurrentUserDto ToCurrentUserDto ( this User user)
    {
        return new CurrentUserDto
        {
            Id = user.Id,
            Email= user.Email,
            FullName=user.FullName,
            Role=user.Role.ToString()
        };
    }
    
}