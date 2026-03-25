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
    .AddInfrastructureServices()
    .AddSwaggerConfiguration()
    .AddCorsConfiguration()
    .AddControllers(options =>
    {
        options.Filters.Add<ValidationFilter>();  //To auto-validate all api calls
    });

var app = builder.Build();

// ── Pipeline ──
app.UseGlobalExceptionHandling();
app.UseSwaggerConfiguration();
app.UseHttpsRedirection();
app.UseCors(CorsExtensions.FrontendPolicy);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// ── Migrate & Seed ──
await app.ApplyMigrationsAsync();

app.Run();