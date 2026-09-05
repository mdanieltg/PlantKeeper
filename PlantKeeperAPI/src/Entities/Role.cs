using System.Diagnostics;
using Microsoft.AspNetCore.Identity;

namespace PlantKeeperAPI.Entities;

/// <summary>
/// A named bundle of permissions. Roles stack: a keeper who moderates the almanac holds
/// both <c>keeper</c> and <c>moderator</c>, and the union of their permissions applies.
/// <para>
/// Nothing in the API authorizes against a role name - authorization always names a
/// permission, which is what lets a role be redefined without touching an attribute.
/// </para>
/// </summary>
[DebuggerDisplay("Role: {Name}")]
public class Role : IdentityRole<Guid>
{
    public required string Description { get; set; }
}
