using Microsoft.EntityFrameworkCore;
using PlantKeeperAPI.Database;

namespace PlantKeeperAPI.Initialization;

public static class Startup
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        var connectionString = environment.EnvironmentName switch
        {
            "Production" => configuration.GetConnectionString("Production"),
            "CAE" => configuration.GetConnectionString("CAE"),
            "QAE" => configuration.GetConnectionString("QAE"),
            _ => configuration.GetConnectionString("Dev")
        };
        return services.AddDbContext<PlantKeeperDbContext>(options =>
            options.UseMySql(connectionString, new MySqlServerVersion("8.0.38")));
    }
}
