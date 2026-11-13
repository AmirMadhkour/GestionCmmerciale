using GestionCommerciale.Application.Common.Exceptions;
using GestionCommerciale.Application.DTOs.Auth;
using GestionCommerciale.Application.Interfaces.Repositories;
using GestionCommerciale.Application.Interfaces.Security;
using GestionCommerciale.Application.Interfaces.Services;
using GestionCommerciale.Application.Mappings;

namespace GestionCommerciale.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public AuthService (IUserRepository userRepository , IPasswordHasher passwordHasher , IJwtTokenGenerator jwtTokenGenerator)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<AuthResponseDto> LoginAsync (LoginDto loginDto)
    {
        var user = await _userRepository.GetByEmailAsync(loginDto.Email);
        if (user == null) 
            throw new UnauthorizedException("Email ou mdp incorrect");
        
        if (!_passwordHasher.VerifyPassword(loginDto.Password, user.PasswordHash))
            throw new UnauthorizedException("Email ou mdp incorrect");
        
        var token = _jwtTokenGenerator.GenerateToken(user);

        return new AuthResponseDto
        {
            Token = token,
            Email = user.Email,
            FullName = user.FullName,
            Role= user.Role.ToString()
        };
    }

    public async Task<CurrentUserDto> CurrentUserAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
            throw new NotFoundException("user introuvable");
        
        return user.ToCurrentUserDto();
    }
}

