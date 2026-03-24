using PrintFlow.API.Extensions;
using PrintFlow.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// ── Services ──
builder.Services
    .AddDatabase(builder.Configuration)
    .AddIdentityConfiguration()
    .AddJwtAuthentication(builder.Configuration)
    .AddInfrastructureServices()
    .AddSwaggerConfiguration()
    .AddCorsConfiguration()
    .AddControllers();

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