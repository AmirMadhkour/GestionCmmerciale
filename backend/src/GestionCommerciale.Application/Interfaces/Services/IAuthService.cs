using GestionCommerciale.Application.DTOs.Auth;

namespace GestionCommerciale.Application.Interfaces.Services;

public interface IAuthService
{
    Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
    Task<CurrentUserDto> CurrentUserAsync (Guid userId);
}

