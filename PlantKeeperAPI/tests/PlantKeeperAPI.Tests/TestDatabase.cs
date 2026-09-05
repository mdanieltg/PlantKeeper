using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql;
using PlantKeeperAPI.Authorization;
using PlantKeeperAPI.Database;

namespace PlantKeeperAPI.Tests;

/// <summary>
/// The database the tests run against: the same PostgreSQL instance as development, a
/// different database on it.
/// <para>
/// It is dropped and re-migrated once per run rather than cleaned between tests. Tenancy is
/// what these tests assert, and tenancy is only observable with rows in the table belonging
/// to somebody else - so the fixture is shared on purpose, and each test reads it rather
/// than reshaping it.
/// </para>
/// </summary>
public static class TestDatabase
{
    public const string Name = "plants_test";

    /// <summary>
    /// The development connection string with the database name swapped.
    /// <para>
    /// Read from the API project's user-secrets, which is why this project declares the
    /// same <c>UserSecretsId</c>. Nothing here is committed, and the tests fail with a
    /// readable message rather than a null reference when the secret is missing.
    /// </para>
    /// </summary>
    public static string ConnectionString { get; } = BuildConnectionString();

    /// <summary>Drops whatever the last run left and applies every migration from scratch.</summary>
    public static void Reset()
    {
        DbContextOptions<PlantKeeperDbContext> options =
            new DbContextOptionsBuilder<PlantKeeperDbContext>()
                .UseNpgsql(ConnectionString)
                .Options;

        using PlantKeeperDbContext dbContext = new(options, CurrentKeeper.None);
        dbContext.Database.EnsureDeleted();
        dbContext.Database.Migrate();
    }

    private static string BuildConnectionString()
    {
        string? development = new ConfigurationBuilder()
            .AddUserSecrets(typeof(TestDatabase).Assembly)
            .AddEnvironmentVariables()
            .Build()
            .GetConnectionString("Dev");

        if (string.IsNullOrWhiteSpace(development))
            throw new InvalidOperationException(
                "No 'Dev' connection string. Set it with: dotnet user-secrets set " +
                "\"ConnectionStrings:Dev\" \"<npgsql connection string>\" --project src/PlantKeeperAPI.csproj");

        return new NpgsqlConnectionStringBuilder(development) { Database = Name }.ConnectionString;
    }
}
