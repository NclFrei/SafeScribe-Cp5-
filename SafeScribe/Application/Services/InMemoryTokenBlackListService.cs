using SafeScribe.Domain.Interfaces;
using System.Collections.Concurrent;

namespace SafeScribe.Application.Services;

public class InMemoryTokenBlacklistService : ITokenBlacklistService
{
    private readonly ConcurrentDictionary<string, DateTime> _blacklistedTokens = new();

    public Task AddToBlacklistAsync(string jti)
    {
        _blacklistedTokens.TryAdd(jti, DateTime.UtcNow);
        return Task.CompletedTask;
    }

    public Task<bool> IsBlacklistedAsync(string jti)
    {
        return Task.FromResult(_blacklistedTokens.ContainsKey(jti));
    }
}