// src/CampusGate.Api/Middleware/CorrelationIdMiddleware.cs
using System.Text.RegularExpressions;

namespace CampusGate.Api.Middleware;

public sealed partial class CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
{
    public const string HeaderName = "X-Correlation-Id";
    public const string ItemKey = "CorrelationId";

    public async Task InvokeAsync(HttpContext context)
    {
        var incoming = context.Request.Headers[HeaderName].ToString();
        var correlationId = IsValid(incoming) ? incoming : Guid.NewGuid().ToString("N");   // ①

        context.Items[ItemKey] = correlationId;                                             // ②
        context.Response.OnStarting(() =>                                                   // ③
        {
            context.Response.Headers[HeaderName] = correlationId;
            return Task.CompletedTask;
        });

        using (logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = correlationId })) // ④
        {
            await next(context);                                                            // ⑤
        }
    }

    private static bool IsValid(string value) =>
        value.Length is > 0 and <= 64 && AllowedCharacters().IsMatch(value);

    [GeneratedRegex("^[A-Za-z0-9-]+$")]
    private static partial Regex AllowedCharacters();                                       // ⑥
}