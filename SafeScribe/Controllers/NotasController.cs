using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SafeScribe.Domain.DTOs.Request;
using SafeScribe.Domain.DTOs.Response;
using SafeScribe.Domain.Models;
using SafeScribe.Infrastructure.Data;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using SafeScribe.Application.Services;

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

    [HttpPost]
    [Authorize(Roles = "Editor,Admin")]
    public async Task<ActionResult<NoteResponseDTO>> Criar(NoteCreateDTO dto)
    {
        var userId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var created = await _service.CreateAsync(dto, userId);
        return CreatedAtAction(nameof(Obter), new { id = created.Id }, created);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<NoteResponseDTO>> Obter(Guid id)
    {
        var userId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var role = User.FindFirstValue(ClaimTypes.Role)!;

        var note = await _service.GetByIdAsync(id, userId, role);
        if (note is null) return Forbid();

        return Ok(note);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Atualizar(Guid id, NoteCreateDTO dto)
    {
        var userId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var role = User.FindFirstValue(ClaimTypes.Role)!;

        var success = await _service.UpdateAsync(id, dto, userId, role);
        if (!success) return Forbid();

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Deletar(Guid id)
    {
        var success = await _service.DeleteAsync(id);
        if (!success) return NotFound();

        return NoContent();
    }
}