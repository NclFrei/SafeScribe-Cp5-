using Microsoft.AspNetCore.Identity;
using SafeScribe.Domain.Enums;

namespace SafeScribe.Domain.DTOs.Request;

public record UserRegisterDTO(string Username, string Password, RoleUser Role);