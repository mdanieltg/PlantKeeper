using System.Diagnostics;

namespace PlantKeeperAPI.DataTransferObjects;

/// <summary>
/// Who the caller is and what they may do.
/// <para>
/// Built by hand rather than through Mapster: roles and permissions are read off the
/// signed-in principal's claims, not off the <c>Keeper</c> entity, so there is no
/// entity-shaped source for a mapping to require.
/// </para>
/// </summary>
[DebuggerDisplay("SignedInKeeper: {UserName}")]
public class SignedInKeeperDto
{
    public required Guid Id { get; set; }

    public required string UserName { get; set; }

    public required string DisplayName { get; set; }

    public string? Email { get; set; }

    /// <summary>Role names, for display. Never authorize against these.</summary>
    public required IEnumerable<string> Roles { get; set; }

    /// <summary>
    /// The union of the permissions carried by those roles. The frontend hides controls
    /// with these; the API is still the authority on every request.
    /// </summary>
    public required IEnumerable<string> Permissions { get; set; }
}
