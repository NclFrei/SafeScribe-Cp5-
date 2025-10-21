using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SafeScribe.Domain.DTOs.Request;
using SafeScribe.Domain.DTOs.Response;
using SafeScribe.Application.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SafeScribe.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class NotasController : ControllerBase
{
    private readonly NoteService _service;

    public NotasController(NoteService service)
    {
        _service = service;
    }

    // ============================================================
    // POST: api/Notas
    // ============================================================
    [HttpPost]
    [Authorize(Roles = "Editor,Admin")]
    public async Task<ActionResult<NoteResponseDTO>> Criar(NoteCreateDTO dto)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized("Token sem claim de usuário.");

        var created = await _service.CreateAsync(dto, userId.Value);
        return CreatedAtAction(nameof(Obter), new { id = created.Id }, created);
    }

    // ============================================================
    // GET: api/Notas/{id}
    // ============================================================
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<NoteResponseDTO>> Obter(Guid id)
    {
        var (userId, role) = GetUserContext();
        if (userId is null)
            return Unauthorized("Token sem claim de usuário.");

        var note = await _service.GetByIdAsync(id, userId.Value, role);
        if (note is null)
            return NotFound();

        return Ok(note);
    }

    // ============================================================
    // PUT: api/Notas/{id}
    // ============================================================
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Atualizar(Guid id, NoteCreateDTO dto)
    {
        var (userId, role) = GetUserContext();
        if (userId is null)
            return Unauthorized("Token sem claim de usuário.");

        var success = await _service.UpdateAsync(id, dto, userId.Value, role);
        if (!success)
            return Forbid();

        return NoContent();
    }

    // ============================================================
    // DELETE: api/Notas/{id}
    // ============================================================
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Deletar(Guid id)
    {
        var success = await _service.DeleteAsync(id);
        if (!success)
            return NotFound();

        return NoContent();
    }

    // ============================================================
    // MÉTODOS AUXILIARES PRIVADOS
    // ============================================================
    private Guid? GetUserId()
    {
        var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirst(ClaimTypes.NameIdentifier);

        return userIdClaim != null ? Guid.Parse(userIdClaim.Value) : (Guid?)null;
    }

    private (Guid?, string) GetUserContext()
    {
        var id = GetUserId();
        var role = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
        return (id, role);
    }
}
