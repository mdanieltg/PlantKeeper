using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PlantKeeperAPI.Authorization;
using PlantKeeperAPI.Entities;

namespace PlantKeeperAPI.Database;

/// <summary>
/// The three roles and the permissions they carry, seeded through the migration so a
/// freshly created database is usable without a separate setup step.
/// <para>
/// Every value here is a fixed literal - identifiers, normalized names, concurrency
/// stamps. <c>HasData</c> diffs the model against what the migration history says is
/// already there, so anything generated at runtime would differ on every model build and
/// produce a new migration each time. <see cref="IdentityRole{TKey}" />'s own constructor
/// assigns a random <c>ConcurrencyStamp</c>, which is exactly that trap.
/// </para>
/// </summary>
public static class IdentitySeedData
{
    public static readonly Guid KeeperRoleId = new("375725bb-c1f6-4991-8c56-4a831d977244");
    public static readonly Guid ModeratorRoleId = new("d43fe141-23f3-4115-89aa-d03755f4da46");
    public static readonly Guid AdminRoleId = new("95c1256a-9c49-4d6b-92e1-6a1a8cfdf2c4");

    public static void Seed(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>().HasData(
            new Role
            {
                Id = KeeperRoleId,
                Name = RoleNames.Keeper,
                NormalizedName = "KEEPER",
                ConcurrencyStamp = "8f2c1d40-keeper-role-seed",
                Description = "Owns a collection of plants and their logs, and may propose almanac changes."
            },
            new Role
            {
                Id = ModeratorRoleId,
                Name = RoleNames.Moderator,
                NormalizedName = "MODERATOR",
                ConcurrencyStamp = "8f2c1d40-moderator-role-seed",
                Description = "Reviews proposed changes to the shared almanac."
            },
            new Role
            {
                Id = AdminRoleId,
                Name = RoleNames.Admin,
                NormalizedName = "ADMIN",
                ConcurrencyStamp = "8f2c1d40-admin-role-seed",
                Description = "Administers keepers and roles."
            });

        // Identifiers are hand-assigned so the set is stable across model builds. Adding a
        // permission means appending with the next number, never renumbering.
        modelBuilder.Entity<IdentityRoleClaim<Guid>>().HasData(
            Claim(1, KeeperRoleId, Permissions.PlantsRead),
            Claim(2, KeeperRoleId, Permissions.PlantsWrite),
            Claim(3, KeeperRoleId, Permissions.AlmanacRead),
            Claim(4, KeeperRoleId, Permissions.AlmanacPropose),
            Claim(5, ModeratorRoleId, Permissions.AlmanacApprove),
            Claim(6, AdminRoleId, Permissions.KeepersManage),
            Claim(7, AdminRoleId, Permissions.RolesManage));
    }

    private static IdentityRoleClaim<Guid> Claim(int id, Guid roleId, string permission) =>
        new()
        {
            Id = id,
            RoleId = roleId,
            ClaimType = Permissions.ClaimType,
            ClaimValue = permission
        };
}
