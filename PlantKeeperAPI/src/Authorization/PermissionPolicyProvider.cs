using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace PlantKeeperAPI.Authorization;

/// <summary>
/// Manufactures an authorization policy for every <see cref="Permissions" /> value on
/// demand, so a new permission is one constant in <see cref="Permissions" /> and nothing
/// else - no matching <c>AddPolicy</c> call to forget.
/// <para>
/// A policy name is a permission only when it starts with <see cref="Prefix" />. Anything
/// else falls through to the default provider, which is what keeps hand-registered
/// policies working alongside these.
/// </para>
/// </summary>
public class PermissionPolicyProvider : IAuthorizationPolicyProvider
{
    /// <summary>Marks a policy name as a permission rather than a hand-registered policy.</summary>
    public const string Prefix = "permission:";

    private readonly DefaultAuthorizationPolicyProvider _fallback;

    public PermissionPolicyProvider(IOptions<AuthorizationOptions> options) =>
        _fallback = new DefaultAuthorizationPolicyProvider(options);

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => _fallback.GetDefaultPolicyAsync();

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() => _fallback.GetFallbackPolicyAsync();

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (!policyName.StartsWith(Prefix, StringComparison.Ordinal))
            return _fallback.GetPolicyAsync(policyName);

        string permission = policyName[Prefix.Length..];

        // RequireAuthenticatedUser as well as the claim: without it an anonymous request
        // fails on the claim and the result is a 403, when the honest answer is 401.
        AuthorizationPolicy policy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .RequireClaim(Permissions.ClaimType, permission)
            .Build();

        return Task.FromResult<AuthorizationPolicy?>(policy);
    }
}
