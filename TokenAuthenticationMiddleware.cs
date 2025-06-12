using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading.Tasks;

public class TokenAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private const string AUTH_HEADER = "Authorization";
    private const string BEARER_PREFIX = "Bearer ";

    // For demonstration, a hardcoded valid token. In production, validate JWT or use a secure method.
    private const string VALID_TOKEN = "your-secure-token-here";

    public TokenAuthenticationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            if (!context.Request.Headers.TryGetValue(AUTH_HEADER, out var authHeader) ||
                !authHeader.Any() ||
                !authHeader.First().StartsWith(BEARER_PREFIX) ||
                authHeader.First().Substring(BEARER_PREFIX.Length) != VALID_TOKEN)
            {
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("{\"error\": \"Unauthorized. Valid token required.\"}");
                return;
            }
        }

        await _next(context);
    }
}