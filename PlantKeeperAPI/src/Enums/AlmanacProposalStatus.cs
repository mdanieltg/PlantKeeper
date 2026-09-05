namespace PlantKeeperAPI.Enums;

public enum AlmanacProposalStatus
{
    /// <summary>Waiting for a moderator. The almanac is unchanged.</summary>
    Pending,

    /// <summary>Written to the almanac. Either auto-approved or approved by a reviewer.</summary>
    Applied,

    /// <summary>Turned down, or refused on review because the target had moved on.</summary>
    Rejected
}
