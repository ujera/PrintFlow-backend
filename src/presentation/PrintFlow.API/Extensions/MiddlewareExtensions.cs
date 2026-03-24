using PrintFlow.API.Middleware;

namespace PrintFlow.API.Extensions;

public static class MiddlewareExtensions
{
    public static WebApplication UseGlobalExceptionHandling(this WebApplication app)
    {
        app.UseMiddleware<GlobalExceptionMiddleware>();
        return app;
    }
}
