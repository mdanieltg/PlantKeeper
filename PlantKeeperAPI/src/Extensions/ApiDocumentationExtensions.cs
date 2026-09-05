using Scalar.AspNetCore;

namespace PlantKeeperAPI.Extensions;

public static class ApiDocumentationExtensions
{
    public static IServiceCollection AddApiDocumentation(this IServiceCollection services) =>
        services.AddOpenApi();

    /// <summary>
    /// Serves the OpenAPI document at <c>/openapi/v1.json</c> and the Scalar reference UI
    /// at <c>/scalar/v1</c>. Development only - the document describes every endpoint,
    /// and there is no authentication in front of it.
    /// </summary>
    public static WebApplication UseApiDocumentation(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment()) return app;

        // AllowAnonymous, because the fallback authorization policy would otherwise put a
        // 401 in front of both. They stay Development-only either way.
        app.MapOpenApi().AllowAnonymous();
        app.MapScalarApiReference(options => options.WithTitle("PlantKeeper API")).AllowAnonymous();

        return app;
    }
}
