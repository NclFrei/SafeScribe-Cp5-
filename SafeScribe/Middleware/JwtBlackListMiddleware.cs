using SafeScribe.Domain.Interfaces;
using System.IdentityModel.Tokens.Jwt;

namespace SafeScribe.Middleware;

public class JwtBlacklistMiddleware
{
    private readonly RequestDelegate _next;
    public JwtBlacklistMiddleware(RequestDelegate next) => _next = next;
    public async Task Invoke(HttpContext ctx, ITokenBlacklistService blacklist)
    {
        if (ctx.User?.Identity?.IsAuthenticated == true)
        {
            var jwtId = ctx.User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
            if (!string.IsNullOrEmpty(jwtId) && await blacklist.IsBlacklistedAsync(jwtId))
            {
                ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await ctx.Response.WriteAsync("Token invalidado (blacklist).");
                return;
            }
        }
        await _next(ctx);
    }
}