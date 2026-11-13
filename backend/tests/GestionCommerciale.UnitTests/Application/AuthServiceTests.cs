using GestionCommerciale.Application.Common.Exceptions;
using GestionCommerciale.Application.DTOs.Auth;
using GestionCommerciale.Application.Interfaces.Repositories;
using GestionCommerciale.Application.Interfaces.Security;
using GestionCommerciale.Application.Services;
using GestionCommerciale.Domain.Entities;
using GestionCommerciale.Domain.Enums;
using Moq;

namespace GestionCommerciale.UniTests.Application;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IJwtTokenGenerator> _jwtGeneratorMock;
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _jwtGeneratorMock = new Mock<IJwtTokenGenerator>();
        _sut = new AuthService(_userRepoMock.Object, _passwordHasherMock.Object, _jwtGeneratorMock.Object);
    }

    private static User CreateSampleUser(UserRole role = UserRole.ADMIN) => new()
    {
        Email = "admin@gestion.com",
        PasswordHash = "hashed_pwd",
        FullName = "System Admin",
        Role = role
    };

    [Fact]
    public async Task LoginAsync_WhenPasswordIsWrong_ThrowsUnauthorizedException()
    {
        var user = CreateSampleUser();
        _userRepoMock.Setup(r => r.GetByEmailAsync("admin@gestion.com")).ReturnsAsync(user);
        _passwordHasherMock.Setup(h => h.VerifyPassword("WrongPassword!", "hashed_pwd")).Returns(false);

        var request = new LoginDto { Email = "admin@gestion.com", Password = "WrongPassword!" };

        await Assert.ThrowsAsync<UnauthorizedException>(() => _sut.LoginAsync(request));
        _jwtGeneratorMock.Verify(j => j.GenerateToken(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_WhenEmailIsUnknown_ThrowsUnauthorizedException()
    {
        _userRepoMock.Setup(r => r.GetByEmailAsync("ghost@gestion.com")).ReturnsAsync((User?)null);

        var request = new LoginDto { Email = "ghost@gestion.com", Password = "Admin123!" };

        await Assert.ThrowsAsync<UnauthorizedException>(() => _sut.LoginAsync(request));
        _jwtGeneratorMock.Verify(j => j.GenerateToken(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_WhenCredentialsAreValid_ReturnsTokenAndRole()
    {
        var user = CreateSampleUser(UserRole.ADMIN);
        _userRepoMock.Setup(r => r.GetByEmailAsync("admin@gestion.com")).ReturnsAsync(user);
        _passwordHasherMock.Setup(h => h.VerifyPassword("Admin123!", "hashed_pwd")).Returns(true);
        _jwtGeneratorMock.Setup(j => j.GenerateToken(user)).Returns("mock-jwt-token");

        var request = new LoginDto { Email = "admin@gestion.com", Password = "Admin123!" };

        var result = await _sut.LoginAsync(request);

        Assert.Equal("mock-jwt-token", result.Token);
        Assert.Equal("ADMIN", result.Role);
        Assert.Equal("admin@gestion.com", result.Email);
        Assert.Equal("System Admin", result.FullName);
    }

    [Fact]
    public async Task CurrentUserAsync_WhenUserExists_ReturnsUserDto()
    {
        var user = CreateSampleUser(UserRole.USER);
        _userRepoMock.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);

        var result = await _sut.CurrentUserAsync(user.Id);

        Assert.Equal(user.Id, result.Id);
        Assert.Equal("USER", result.Role);
    }

    [Fact]
    public async Task CurrentUserAsync_WhenUserDoesNotExist_ThrowsNotFoundException()
    {
        var id = Guid.NewGuid();
        _userRepoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.CurrentUserAsync(id));
    }
}
