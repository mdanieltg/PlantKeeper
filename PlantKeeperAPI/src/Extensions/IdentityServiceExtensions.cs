using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PlantKeeperAPI.Authorization;
using PlantKeeperAPI.Database;
using PlantKeeperAPI.Entities;

namespace PlantKeeperAPI.Extensions;

public static class IdentityServiceExtensions
{
    /// <summary>
    /// Registers the Identity stores only - <see cref="UserManager{TUser}" /> and
    /// <see cref="RoleManager{TRole}" /> over the application DbContext.
    /// <para>
    /// Deliberately <c>AddIdentityCore</c> rather than <c>AddIdentity</c>: this brings no
    /// authentication scheme, no sign-in manager and no cookie, so adding it changes
    /// nothing about how existing endpoints answer. Sign-in and enforcement arrive with
    /// the auth pipeline.
    /// </para>
    /// </summary>
    public static IServiceCollection AddIdentityFoundation(this IServiceCollection services)
    {
        services.AddIdentityCore<Keeper>(options =>
            {
                // A keeper signs in by user name or passkey; e-mail is optional contact
                // detail, so uniqueness across an unset value would reject the second keeper.
                options.User.RequireUniqueEmail = false;

                options.Password.RequiredLength = 12;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
            .AddRoles<Role>()
            .AddEntityFrameworkStores<PlantKeeperDbContext>();

        return services;
    }

    /// <summary>
    /// Creates the first keeper if none exists, holding all three seeded roles.
    /// <para>
    /// The roles themselves are seeded by the migration; a keeper is not, because
    /// <c>HasData</c> would have to hard-code a security stamp and a normalized user name,
    /// and those belong to <see cref="UserManager{TUser}" />. The keeper is created with no
    /// password and no passkey - credentials are enrolled separately.
    /// </para>
    /// <para>
    /// Skipped with a warning rather than throwing when the schema is not up to date.
    /// Migrations are never applied automatically here, so a database that is merely
    /// un-migrated must not stop the application from starting.
    /// </para>
    /// </summary>
    public static async Task SeedFirstKeeperAsync(this WebApplication app)
    {
        await using AsyncServiceScope scope = app.Services.CreateAsyncScope();
        IServiceProvider services = scope.ServiceProvider;
        ILogger logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("PlantKeeperAPI.Seed");

        PlantKeeperDbContext dbContext = services.GetRequiredService<PlantKeeperDbContext>();

        if (!await dbContext.Database.CanConnectAsync())
        {
            logger.LogWarning("Skipping keeper seed: the database is not reachable.");
            return;
        }

        if ((await dbContext.Database.GetPendingMigrationsAsync()).Any())
        {
            logger.LogWarning("Skipping keeper seed: migrations are pending. Run 'dotnet ef database update'.");
            return;
        }

        UserManager<Keeper> keepers = services.GetRequiredService<UserManager<Keeper>>();
        if (await keepers.Users.AnyAsync()) return;

        IConfiguration configuration = services.GetRequiredService<IConfiguration>();
        Keeper keeper = new()
        {
            UserName = configuration["Bootstrap:Keeper:UserName"] ?? "keeper",
            DisplayName = configuration["Bootstrap:Keeper:DisplayName"] ?? "Keeper",
            Email = configuration["Bootstrap:Keeper:Email"],
            CreatedAt = DateTimeOffset.UtcNow
        };

        IdentityResult created = await keepers.CreateAsync(keeper);
        if (!created.Succeeded)
            throw new InvalidOperationException(
                $"Could not seed the first keeper: {string.Join("; ", created.Errors.Select(error => error.Description))}");

        IdentityResult assigned = await keepers.AddToRolesAsync(keeper,
            [RoleNames.Keeper, RoleNames.Moderator, RoleNames.Admin]);
        if (!assigned.Succeeded)
            throw new InvalidOperationException(
                $"Could not assign roles to the first keeper: {string.Join("; ", assigned.Errors.Select(error => error.Description))}");

        logger.LogInformation("Seeded first keeper '{UserName}' with all three roles and no credentials yet.",
            keeper.UserName);
    }
}
