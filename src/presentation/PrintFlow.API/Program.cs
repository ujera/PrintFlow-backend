using PrintFlow.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

// ── Services ──
builder.Services
    .AddDatabase(builder.Configuration)
    .AddIdentityConfiguration()
    .AddJwtAuthentication(builder.Configuration)
    .AddSwaggerConfiguration()
    .AddCorsConfiguration()
    .AddControllers();

var app = builder.Build();

// ── Pipeline ──
app.UseSwaggerConfiguration();
app.UseHttpsRedirection();
app.UseCors(CorsExtensions.FrontendPolicy);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// ── Migrate & Seed ──
await app.ApplyMigrationsAsync();

app.Run();