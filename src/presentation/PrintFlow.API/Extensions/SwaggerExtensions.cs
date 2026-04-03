using System.Reflection;
using Microsoft.OpenApi.Models;

namespace PrintFlow.API.Extensions;

public static class SwaggerExtensions
{
    public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "PrintFlow API",
                Version = "v1",
                Description = "Custom Print Shop Order Management System — REST API for managing product catalog, customer orders, payments, and notifications.",
                Contact = new OpenApiContact
                {
                    Name = "PrintFlow Team"
                }
            });

            // JWT auth
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Login via /api/auth/login, then paste the accessToken here."
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });

            // File upload support
            options.MapType<IFormFile>(() => new OpenApiSchema
            {
                Type = "string",
                Format = "binary"
            });

            // Load XML docs from API project
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath);
            }

            // Sort endpoints by tag
            options.OrderActionsBy(api => api.GroupName ?? api.RelativePath);
        });

        return services;
    }

    public static WebApplication UseSwaggerConfiguration(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "PrintFlow API v1");
                options.RoutePrefix = "swagger";
                options.DocumentTitle = "PrintFlow API Documentation";
                options.DefaultModelsExpandDepth(1);
            });
        }

        return app;
    }
}