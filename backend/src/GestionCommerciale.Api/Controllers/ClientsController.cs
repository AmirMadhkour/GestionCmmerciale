using GestionCommerciale.Application.DTOs.Clients;
using GestionCommerciale.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionCommerciale.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize] 
public class ClientsController : ControllerBase
{
    private readonly IClientService _clientService;

    public ClientsController(IClientService clientService)
    {
        _clientService = clientService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ClientDto>>> GetAll()
    {
        return Ok(await _clientService.GetAllAsync());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ClientDto>> GetById(Guid id)
    {
        return Ok(await _clientService.GetByIdAsync(id));
    }

   
    [HttpPost]
    [Authorize(Roles = "ADMIN")] 
    public async Task<ActionResult<ClientDto>> Create(CreateClientDto dto)
    {
        var client = await _clientService.CreateClientAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = client.Id }, client);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<ActionResult> Update(Guid id, UpdateClientDto dto)
    {
        await _clientService.UpdateClientAsync(id, dto);
        return NoContent(); 
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<ActionResult> Delete(Guid id)
    {
        await _clientService.DeleteClientAsync(id);
        return NoContent();
    }
}