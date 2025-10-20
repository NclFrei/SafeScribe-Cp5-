namespace SafeScribe.Domain.Interfaces;

public interface ITokenBlacklistService
{

    Task AddToBlacklistAsync(string jti);

    Task<bool> IsBlacklistedAsync(string jti);
}
