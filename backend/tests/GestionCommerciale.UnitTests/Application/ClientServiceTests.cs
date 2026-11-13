using GestionCommerciale.Application.Common.Exceptions;
using GestionCommerciale.Application.DTOs.Clients;
using GestionCommerciale.Application.Interfaces.Repositories;
using GestionCommerciale.Application.Services;
using GestionCommerciale.Domain.Entities;
using Moq;

namespace GestionCommerciale.UniTests.Application;

public class ClientServiceTests
{
    private readonly Mock<IClientRepository> _clientRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly ClientService _sut;

    public ClientServiceTests()
    {
        _clientRepoMock = new Mock<IClientRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _sut = new ClientService(_clientRepoMock.Object, _unitOfWorkMock.Object);
    }

    private static Client CreateSampleClient(string email = "amir@test.com") =>
        Client.Create("Madhkour", "Amir", email, "56103009", new[] { "Tunis" });

    private static CreateClientDto CreateDto(string email = "Amir@Test.com") => new()
    {
        LastName = "Madhkour",
        FirstNameOrCompany = "Amir",
        Email = email,
        Phone = "56103009",
        Address = new[] { "Tunis" }
    };

    [Fact]
    public async Task CreateClientAsync_WhenEmailIsUnique_CreatesClientWithLowerCasedEmail()
    {
        _clientRepoMock.Setup(r => r.EmailExistsAsync(It.IsAny<string>())).ReturnsAsync(false);

        var result = await _sut.CreateClientAsync(CreateDto("Amir@Test.com"));

        Assert.Equal("amir@test.com", result.Email);
        Assert.Equal("Madhkour", result.LastName);
        _clientRepoMock.Verify(r => r.AddAsync(It.IsAny<Client>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateClientAsync_WhenEmailAlreadyExists_ThrowsConflictException()
    {
        _clientRepoMock.Setup(r => r.EmailExistsAsync(It.IsAny<string>())).ReturnsAsync(true);

        await Assert.ThrowsAsync<ConflictException>(() => _sut.CreateClientAsync(CreateDto()));

        _clientRepoMock.Verify(r => r.AddAsync(It.IsAny<Client>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetByIdAsync_WhenClientDoesNotExist_ThrowsNotFoundException()
    {
        var id = Guid.NewGuid();
        _clientRepoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Client?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetByIdAsync(id));
    }

    [Fact]
    public async Task UpdateClientAsync_WhenNewEmailBelongsToAnotherClient_ThrowsConflictException()
    {
        var client = CreateSampleClient("amir@test.com");
        _clientRepoMock.Setup(r => r.GetByIdAsync(client.Id)).ReturnsAsync(client);
        _clientRepoMock.Setup(r => r.EmailExistsAsync("other@test.com")).ReturnsAsync(true);

        var dto = new UpdateClientDto
        {
            LastName = "Madhkour",
            FirstNameOrCompany = "Amir",
            Email = "other@test.com",
            Phone = "56103009",
            Address = new[] { "Tunis" }
        };

        await Assert.ThrowsAsync<ConflictException>(() => _sut.UpdateClientAsync(client.Id, dto));

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateClientAsync_WhenEmailIsUnchangedExceptCase_DoesNotCheckUniquenessAndSaves()
    {
        var client = CreateSampleClient("amir@test.com");
        _clientRepoMock.Setup(r => r.GetByIdAsync(client.Id)).ReturnsAsync(client);

        var dto = new UpdateClientDto
        {
            LastName = "Nouveau",
            FirstNameOrCompany = "Amir",
            Email = "AMIR@TEST.COM",
            Phone = "56103009",
            Address = new[] { "Ariana" }
        };

        await _sut.UpdateClientAsync(client.Id, dto);

        Assert.Equal("Nouveau", client.LastName);
        _clientRepoMock.Verify(r => r.EmailExistsAsync(It.IsAny<string>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteClientAsync_WhenClientHasOrders_ThrowsConflictException()
    {
        var client = CreateSampleClient();
        _clientRepoMock.Setup(r => r.GetByIdAsync(client.Id)).ReturnsAsync(client);
        _clientRepoMock.Setup(r => r.HasOrdersAsync(client.Id)).ReturnsAsync(true);

        await Assert.ThrowsAsync<ConflictException>(() => _sut.DeleteClientAsync(client.Id));

        _clientRepoMock.Verify(r => r.Remove(It.IsAny<Client>()), Times.Never);
    }

    [Fact]
    public async Task DeleteClientAsync_WhenClientHasNoOrders_RemovesAndSaves()
    {
        var client = CreateSampleClient();
        _clientRepoMock.Setup(r => r.GetByIdAsync(client.Id)).ReturnsAsync(client);
        _clientRepoMock.Setup(r => r.HasOrdersAsync(client.Id)).ReturnsAsync(false);

        await _sut.DeleteClientAsync(client.Id);

        _clientRepoMock.Verify(r => r.Remove(client), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
