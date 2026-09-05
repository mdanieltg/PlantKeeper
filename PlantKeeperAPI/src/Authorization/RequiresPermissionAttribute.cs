using Microsoft.AspNetCore.Authorization;

namespace PlantKeeperAPI.Authorization;

/// <summary>
/// Requires a single <see cref="Permissions" /> value, e.g.
/// <c>[RequiresPermission(Permissions.PlantsWrite)]</c>.
/// <para>
/// Stacking two of these requires <em>both</em> - each attribute is a separate policy and
/// every one has to pass. There is deliberately no "any of" form: the roles are the union
/// mechanism, and a keeper who needs two privileges is given a role that carries both.
/// </para>
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class RequiresPermissionAttribute : AuthorizeAttribute
{
    public RequiresPermissionAttribute(string permission) =>
        Policy = PermissionPolicyProvider.Prefix + permission;
}
