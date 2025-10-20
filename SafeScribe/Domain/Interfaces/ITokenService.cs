using SafeScribe.Domain.Models;

namespace SafeScribe.Domain.Interfaces;

public interface ITokenService
{
    public string GenerateToken(User user);
}
