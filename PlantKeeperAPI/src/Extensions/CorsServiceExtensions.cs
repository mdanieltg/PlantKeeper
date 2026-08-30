namespace PlantKeeperAPI.Extensions;

public static class CorsServiceExtensions
{
    /// <summary>Permissive policy, applied in Development only.</summary>
    public const string DevelopmentPolicy = "devenv";

    public static IServiceCollection AddCorsPolicies(this IServiceCollection services) =>
        services.AddCors(options =>
        {
            options.AddPolicy(DevelopmentPolicy, policyBuilder => policyBuilder
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowAnyOrigin());
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
