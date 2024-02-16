namespace Resources;

public class CustomLoggingMiddleware
{
    private readonly ILogger _logger;
    private readonly RequestDelegate _next;

    public CustomLoggingMiddleware(RequestDelegate next, ILogger<CustomLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            _logger.LogInformation($"Request: {context.Request.Path} received");
            await _next(context);
            _logger.LogInformation($"Responded with {context.Response.StatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while Fetching resource");
            throw;
        }
    }
}

public static class TestMiddlewareExtension
{
    public static IApplicationBuilder UseCustomLoggingMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<CustomLoggingMiddleware>();
    }
}