using Microsoft.OpenApi;

namespace LocalLink.API.Extensions;

public static class SwaggerExtensions
{
    public static IServiceCollection AddAppSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "InterLink Banking API",
                Version = "v1",
                Description = "Cloud-native Connected Regional Banking & Local Services Platform API"
            });

            var securityScheme = new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Description = "Enter JWT Bearer token format: Bearer {your_token_here}",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
            };

            options.AddSecurityDefinition("Bearer", securityScheme);

            var securitySchemeRef = new OpenApiSecuritySchemeReference("Bearer");

            options.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
            {
                {
                    securitySchemeRef,
                    new List<string>()
                }
            });
        });

        return services;
    }

    public static IApplicationBuilder UseAppSwagger(this IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "InterLink Banking API v1");
                c.RoutePrefix = "swagger";
                c.DisplayRequestDuration();
            });
        }

        return app;
    }
}
