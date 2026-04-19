using PrintFlow.API.Extensions;
using PrintFlow.API.Middleware;
using PrintFlow.Application;
using PrintFlow.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// ── Services ──
builder.Services
    .AddDatabase(builder.Configuration)
    .AddIdentityConfiguration()
    .AddJwtAuthentication(builder.Configuration)
    .AddApplicationServices()
    .AddInfrastructureServices(builder.Configuration)
    .AddSwaggerConfiguration()
    .AddCorsConfiguration()
    .AddControllers(options =>
    {
        options.Filters.Add<ValidationFilter>();
    });

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 10 * 1024 * 1024; // 10MB
});

var app = builder.Build();

// ── Pipeline ──
app.UseGlobalExceptionHandling();
app.UseSwaggerConfiguration();
app.UseHttpsRedirection();
app.UseCors(CorsExtensions.FrontendPolicy);

// Serve uploaded files from wwwroot/uploads
app.UseStaticFiles();


app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// ── Migrate & Seed ──
await app.ApplyMigrationsAsync();

app.Run();

public partial class Program { }