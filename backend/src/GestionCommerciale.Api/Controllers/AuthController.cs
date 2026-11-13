using GestionCommerciale.Application.DTOs.Auth;
using GestionCommerciale.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GestionCommerciale.Api.Controllers;

[Route("api/[Controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService ;
    public AuthController ( IAuthService authS)
    {
        _authService = authS;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponseDto>> Login ( LoginDto dto)
    {
        var response = await _authService.LoginAsync(dto);
        return Ok(response);
    }

    [HttpGet("me")]
    [Authorize]
    public ActionResult GetMe()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = User.FindFirstValue(ClaimTypes.Email);
        var role = User.FindFirstValue(ClaimTypes.Role);

        return Ok (
            new
            {
                id = userId,
                Email = email,
                Role = role
            }
        );
    }
}