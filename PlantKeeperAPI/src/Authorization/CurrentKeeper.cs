using System.Security.Claims;

namespace PlantKeeperAPI.Authorization;

/// <summary>
/// Who the request belongs to. Resolved from the signed-in principal, and the value the
/// global query filters compare against.
/// </summary>
public interface ICurrentKeeper
{
    /// <summary>
    /// The signed-in keeper's identifier, or <see cref="Guid.Empty" /> when there is no
    /// request or nobody is signed in. Empty never matches a row, so the filters fail
    /// closed rather than open.
    /// </summary>
    Guid Id { get; }

    bool IsSignedIn { get; }

    /// <summary>
    /// Whether the caller holds a <see cref="Permissions" /> value. Read off the same claims
    /// the authorization policies use, so a service asking this and an endpoint attribute
    /// gating on it can never disagree.
    /// </summary>
    bool HasPermission(string permission);
}

/// <summary>
/// Reads the identifier off the ambient principal on each access rather than capturing it
/// once. The DbContext is constructed by the container, which can happen before or after
/// authentication depending on what asks for it first; reading late means the filters see
/// the same keeper the controller does, whatever that order turns out to be.
/// </summary>
public class CurrentKeeper : ICurrentKeeper
{
    /// <summary>Nobody. Used at design time, where no request exists and no query runs.</summary>
    public static readonly ICurrentKeeper None = new NoKeeper();

    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentKeeper(IHttpContextAccessor httpContextAccessor) =>
        _httpContextAccessor = httpContextAccessor;

    public Guid Id =>
        Guid.TryParse(_httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier),
            out Guid id)
            ? id
            : Guid.Empty;

    public bool IsSignedIn => Id != Guid.Empty;

    public bool HasPermission(string permission) =>
        _httpContextAccessor.HttpContext?.User.HasClaim(Permissions.ClaimType, permission) ?? false;

    private sealed class NoKeeper : ICurrentKeeper
    {
        public Guid Id => Guid.Empty;
        public bool IsSignedIn => false;
        public bool HasPermission(string permission) => false;
    }
}
