using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PlantKeeperAPI.Authorization;
using PlantKeeperAPI.Database;
using PlantKeeperAPI.Entities;

namespace PlantKeeperAPI.Extensions;

public static class IdentityServiceExtensions
{
    /// <summary>
    /// Identity's managers over the application DbContext - <see cref="UserManager{TUser}" />,
    /// <see cref="RoleManager{TRole}" /> and <see cref="SignInManager{TUser}" />.
    /// <para>
    /// Still <c>AddIdentityCore</c> rather than <c>AddIdentity</c>, which would register an
    /// authentication scheme and a cookie of its own. Those are configured explicitly in
    /// <see cref="AuthenticationServiceExtensions.AddCookieAuthentication" /> instead, so
    /// the API's cookie behaviour - status codes rather than login redirects - is declared
    /// in one place rather than half-inherited from the defaults.
    /// </para>
    /// </summary>
    public static IServiceCollection AddIdentityFoundation(this IServiceCollection services)
    {
        services.AddIdentityCore<Keeper>(options =>
            {
                // A keeper signs in by user name; e-mail is optional contact detail, so
                // uniqueness across an unset value would reject the second keeper.
                options.User.RequireUniqueEmail = false;

                options.Password.RequiredLength = 12;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
            .AddRoles<Role>()
            .AddEntityFrameworkStores<PlantKeeperDbContext>()
            .AddSignInManager();

        // SignInManager reads the ambient request to issue and clear the cookie.
        services.AddHttpContextAccessor();

        return services;
    }

    /// <summary>
    /// Creates the first keeper if none exists, holding all three seeded roles.
    /// <para>
    /// The roles themselves are seeded by the migration; a keeper is not, because
    /// <c>HasData</c> would have to hard-code a security stamp and a normalized user name,
    /// and those belong to <see cref="UserManager{TUser}" />. The keeper is created with no
    /// password: the first one is set through the bootstrap endpoint, which is open only
    /// while the hash is still null.
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

        logger.LogInformation("Seeded first keeper '{UserName}' with all three roles and no password yet.",
            keeper.UserName);
    }
}
