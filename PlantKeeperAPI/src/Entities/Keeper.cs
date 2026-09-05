using System.Diagnostics;
using Microsoft.AspNetCore.Identity;

namespace PlantKeeperAPI.Entities;

/// <summary>
/// A person who keeps plants. Identity owns the credential columns - user name, password
/// hash, security stamp, passkeys - and this type adds only what the domain needs on top.
/// <para>
/// It carries no navigation to <see cref="Plant" /> yet. Ownership arrives with the
/// <c>KeeperId</c> columns and their query filters; adding a navigation here early would
/// pull an unwanted foreign key into the Identity migration.
/// </para>
/// </summary>
[DebuggerDisplay("Keeper: {UserName}")]
public class Keeper : IdentityUser<Guid>
{
    /// <summary>Shown in the UI. The user name is for signing in; this is for reading.</summary>
    public required string DisplayName { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
