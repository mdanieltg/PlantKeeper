namespace PlantKeeperAPI.Authorization;

/// <summary>
/// Every privilege the API can check, as a claim value.
/// <para>
/// Endpoints authorize against these, never against a role name. Roles are bundles
/// (see <see cref="RoleNames" />), so "who may approve an almanac change" can be
/// re-answered by moving a permission between roles rather than by editing attributes
/// across the controllers.
/// </para>
/// </summary>
public static class Permissions
{
    /// <summary>Claim type carrying a permission value on a role or a signed-in keeper.</summary>
    public const string ClaimType = "permission";

    /// <summary>Read plants and logs. Scoped to the keeper's own collection by query filter.</summary>
    public const string PlantsRead = "plants.read";

    /// <summary>Create, update and delete plants and logs in the keeper's own collection.</summary>
    public const string PlantsWrite = "plants.write";

    /// <summary>Read the shared almanac. Every keeper has this.</summary>
    public const string AlmanacRead = "almanac.read";

    /// <summary>Submit an almanac change for review.</summary>
    public const string AlmanacPropose = "almanac.propose";

    /// <summary>
    /// Approve or reject a proposed almanac change. Holding this also means a proposal
    /// applies immediately instead of queueing - the reviewer would only approve it.
    /// </summary>
    public const string AlmanacApprove = "almanac.approve";

    /// <summary>Create, disable and delete keepers.</summary>
    public const string KeepersManage = "keepers.manage";

    /// <summary>Change which permissions a role carries, and which roles a keeper holds.</summary>
    public const string RolesManage = "roles.manage";
}

/// <summary>
/// The seeded roles. These are names for sets of <see cref="Permissions" />, not
/// privileges in themselves, and they are additive rather than hierarchical - an admin
/// who also keeps plants holds both roles.
/// </summary>
public static class RoleNames
{
    /// <summary>Owns a collection. The baseline every signed-in person has.</summary>
    public const string Keeper = "keeper";

    /// <summary>Reviews proposed changes to the shared almanac.</summary>
    public const string Moderator = "moderator";

    /// <summary>Administers keepers and roles. Deliberately carries no plant or almanac rights.</summary>
    public const string Admin = "admin";
}
