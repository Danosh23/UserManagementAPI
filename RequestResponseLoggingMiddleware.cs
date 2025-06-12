using Microsoft.AspNetCore.Http;
using System.Diagnostics;
using System.Threading.Tasks;

public class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public RequestResponseLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var method = context.Request.Method;
        var path = context.Request.Path;

        // Log incoming request
        Console.WriteLine($"[Request] {method} {path}");

        await _next(context);

        stopwatch.Stop();
        var statusCode = context.Response.StatusCode;

        // Log outgoing response
        Console.WriteLine($"[Response] {method} {path} responded {statusCode} in {stopwatch.ElapsedMilliseconds}ms");
    }
}