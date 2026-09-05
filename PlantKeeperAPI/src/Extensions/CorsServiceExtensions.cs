namespace PlantKeeperAPI.Extensions;

public static class CorsServiceExtensions
{
    /// <summary>Applied in Development only.</summary>
    public const string DevelopmentPolicy = "devenv";

    /// <summary>Where <c>ng serve</c> runs.</summary>
    private static readonly string[] DevelopmentOrigins = ["http://localhost:4200", "https://localhost:4200"];

    public static IServiceCollection AddCorsPolicies(this IServiceCollection services) =>
        services.AddCors(options =>
        {
            // AllowAnyOrigin is gone on purpose. The session is a cookie, so the browser
            // only sends it when the response allows credentials - and the CORS spec
            // forbids pairing credentials with a wildcard origin. Listing the dev server
            // explicitly is what makes signed-in requests work from ng serve at all.
            options.AddPolicy(DevelopmentPolicy, policyBuilder => policyBuilder
                .AllowAnyHeader()
                .AllowAnyMethod()
                .WithOrigins(DevelopmentOrigins)
                .AllowCredentials());
        });

    public static WebApplication UseCorsPolicies(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
            app.UseCors(DevelopmentPolicy);
        else
            app.UseCors();

        return app;
    }
}
