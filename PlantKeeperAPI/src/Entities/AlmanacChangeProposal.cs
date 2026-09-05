using System.Diagnostics;
using PlantKeeperAPI.Enums;

namespace PlantKeeperAPI.Entities;

/// <summary>
/// One proposed change to the shared almanac, and what became of it.
/// <para>
/// Every almanac write produces one of these, including the ones that apply straight away -
/// so the table is the almanac's history as well as its review queue. A keeper holding
/// <c>almanac.approve</c> never waits; their rows are recorded
/// <see cref="AutoApproved" /> and applied in the same request.
/// </para>
/// </summary>
[DebuggerDisplay("{Operation} {TargetType}: {Status}")]
public class AlmanacChangeProposal
{
    public Guid Id { get; set; }

    /// <summary>The entity type name, e.g. <c>PlantSpecies</c>. Not a table name.</summary>
    public required string TargetType { get; set; }

    /// <summary>The row being changed, or null for a create - it has no id yet.</summary>
    public Guid? TargetId { get; set; }

    public AlmanacChangeOperation Operation { get; set; }

    /// <summary>
    /// The request body as JSON, in the shape of the matching <c>Input</c> model. Null for a
    /// delete, which proposes no new state.
    /// </summary>
    public string? ProposedState { get; set; }

    /// <summary>
    /// <see cref="IAlmanacVersioned.Version" /> as it stood when this was written. Null for
    /// a create. Approval refuses if the row has moved on since.
    /// </summary>
    public int? TargetVersion { get; set; }

    public AlmanacProposalStatus Status { get; set; }

    /// <summary>Applied without review, because the proposer could have approved it anyway.</summary>
    public bool AutoApproved { get; set; }

    public Guid ProposedById { get; set; }
    public DateTimeOffset ProposedAt { get; set; }

    public Guid? ReviewedById { get; set; }
    public DateTimeOffset? ReviewedAt { get; set; }

    /// <summary>Why a reviewer decided as they did, or why the system refused.</summary>
    public string? ReviewNote { get; set; }
}
