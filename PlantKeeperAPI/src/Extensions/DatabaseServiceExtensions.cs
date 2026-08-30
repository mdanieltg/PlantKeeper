using Microsoft.EntityFrameworkCore;
using PlantKeeperAPI.Database;

namespace PlantKeeperAPI.Extensions;

public static class DatabaseServiceExtensions
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        string? connectionString = environment.EnvironmentName switch
        {
            "Production" => configuration.GetConnectionString("Production"),
            "CAE" => configuration.GetConnectionString("CAE"),
            "QAE" => configuration.GetConnectionString("QAE"),
            _ => configuration.GetConnectionString("Dev")
        };
        return services.AddDbContext<PlantKeeperDbContext>(options =>
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
    }
}
