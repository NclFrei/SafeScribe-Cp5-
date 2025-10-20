using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SafeScribe.Domain.DTOs.Request;
using SafeScribe.Domain.DTOs.Response;
using SafeScribe.Domain.Interfaces;
using SafeScribe.Domain.Models;
using SafeScribe.Infrastructure.Data;
using System;
using System.IdentityModel.Tokens.Jwt;
using SafeScribe.Application.Services;

namespace SafeScribe.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    private readonly ITokenBlacklistService _blacklist;

    public AuthController(AuthService authService, ITokenBlacklistService blacklist)
    {
        _authService = authService;
        _blacklist = blacklist;
    }

    [HttpPost("registrar")]
    [AllowAnonymous]
    public async Task<IActionResult> Registrar(UserRegisterDTO dto)
    {
        var (success, user) = await _authService.RegisterAsync(dto);
        if (!success)
            return Conflict("Usuário já existe.");

        return Created("", new { user!.Id, user.Username, user.Role });
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponseDTO>> Login(LoginRequestDTO dto)
    {
        var result = await _authService.LoginAsync(dto);
        if (result is null)
            return Unauthorized();

        return Ok(result);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var jti = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
        if (string.IsNullOrEmpty(jti)) return BadRequest();

        await _blacklist.AddToBlacklistAsync(jti);
        return Ok(new { message = "Logout efetuado. Token adicionado à blacklist." });
    }
}