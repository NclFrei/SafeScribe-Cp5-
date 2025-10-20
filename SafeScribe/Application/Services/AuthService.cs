using SafeScribe.Domain.DTOs.Request;
using SafeScribe.Domain.DTOs.Response;
using SafeScribe.Domain.Interfaces;
using SafeScribe.Domain.Models;

namespace SafeScribe.Application.Services;

public class AuthService
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;

    public AuthService(IUserRepository userRepository, ITokenService tokenService)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
    }

    public async Task<LoginResponseDTO?> LoginAsync(LoginRequestDTO dto)
    {
        var user = await _userRepository.GetByUsernameAsync(dto.Username);
        if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return null;

        var token = _tokenService.GenerateToken(user);
        return new LoginResponseDTO(token);
    }

    public async Task<(bool success, User? user)> RegisterAsync(UserRegisterDTO dto)
    {
        if (await _userRepository.ExistsByUsernameAsync(dto.Username))
            return (false, null);

        var hash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
        var user = new User { Username = dto.Username, PasswordHash = hash, Role = dto.Role };

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        return (true, user);
    }
}
