using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using PlantKeeperAPI.Authorization;
using PlantKeeperAPI.Entities;

namespace PlantKeeperAPI.Tests;

/// <summary>The two keepers every tenancy test needs: one to be, and one to be kept out of.</summary>
public static class TestKeepers
{
    public const string Password = "TestKeeperPassword1!";
    public const string A = "keeper";
    public const string B = "keeper-b";

    /// <summary>
    /// A keeper nobody shares. Signing out rotates the security stamp, which revokes every
    /// ticket that keeper holds - so a test that signs out has to own its account outright
    /// or it will sign the shared sessions out from under the rest of the fixture.
    /// </summary>
    public const string Disposable = "keeper-c";

    /// <summary>
    /// Gives the seeded keeper a password and creates the second one beside it.
    /// <para>
    /// <c>SeedFirstKeeperAsync</c> has already run by the time the host is up, so keeper A
    /// exists but has no password - it is created deliberately without one. Both go through
    /// <see cref="UserManager{TUser}" /> rather than raw SQL so the hash, security stamp and
    /// normalized name are whatever the real sign-in path expects to find.
    /// </para>
    /// </summary>
    public static async Task EnsureAsync(PlantKeeperApiFactory factory)
    {
        await using AsyncServiceScope scope = factory.Services.CreateAsyncScope();
        UserManager<Keeper> keepers = scope.ServiceProvider.GetRequiredService<UserManager<Keeper>>();

        Keeper first = await keepers.FindByNameAsync(A)
                       ?? throw new InvalidOperationException($"'{A}' was not seeded at startup.");

        if (!await keepers.HasPasswordAsync(first)) Check(await keepers.AddPasswordAsync(first, Password));

        if (await keepers.FindByNameAsync(B) is null)
        {
            Keeper second = new()
            {
                UserName = B,
                DisplayName = "Second Keeper",
                CreatedAt = DateTimeOffset.UtcNow
            };

            Check(await keepers.CreateAsync(second, Password));

            // The keeper role only - no moderator, no admin. Enough to own a collection,
            // which is all a tenancy test needs it to do.
            Check(await keepers.AddToRoleAsync(second, RoleNames.Keeper));
        }

        if (await keepers.FindByNameAsync(Disposable) is null)
        {
            Keeper spare = new()
            {
                UserName = Disposable,
                DisplayName = "Disposable Keeper",
                CreatedAt = DateTimeOffset.UtcNow
            };

            Check(await keepers.CreateAsync(spare, Password));
            Check(await keepers.AddToRoleAsync(spare, RoleNames.Keeper));
        }
    }

    /// <summary>Creates a keeper for one test to own and destroy.</summary>
    public static async Task<Guid> CreateAsync(PlantKeeperApiFactory factory, string userName)
    {
        await using AsyncServiceScope scope = factory.Services.CreateAsyncScope();
        UserManager<Keeper> keepers = scope.ServiceProvider.GetRequiredService<UserManager<Keeper>>();

        Keeper keeper = new()
        {
            UserName = userName,
            DisplayName = userName,
            CreatedAt = DateTimeOffset.UtcNow
        };

        Check(await keepers.CreateAsync(keeper, Password));
        Check(await keepers.AddToRoleAsync(keeper, RoleNames.Keeper));

        return keeper.Id;
    }

    /// <summary>
    /// Deletes a keeper through <see cref="UserManager{TUser}" />, which is what an admin
    /// endpoint will eventually do. The cascade is the database's, not EF's - nothing here
    /// loads the tree first.
    /// </summary>
    public static async Task DeleteAsync(PlantKeeperApiFactory factory, Guid keeperId)
    {
        await using AsyncServiceScope scope = factory.Services.CreateAsyncScope();
        UserManager<Keeper> keepers = scope.ServiceProvider.GetRequiredService<UserManager<Keeper>>();

        Keeper keeper = await keepers.FindByIdAsync(keeperId.ToString())
                        ?? throw new InvalidOperationException($"No keeper '{keeperId}'.");

        Check(await keepers.DeleteAsync(keeper));
    }

    private static void Check(IdentityResult result)
    {
        if (result.Succeeded) return;

        throw new InvalidOperationException(
            string.Join("; ", result.Errors.Select(error => error.Description)));
    }
}
