using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using PlantKeeperAPI.Enums;

namespace PlantKeeperAPI.DataTransferObjects;

/// <summary>
/// A proposed almanac change as the queue shows it.
/// <para>
/// Built by hand rather than through Mapster: <see cref="ProposedState" /> is parsed JSON
/// rather than the entity's string column, so a reviewer sees the change itself instead of
/// an escaped blob.
/// </para>
/// </summary>
[DebuggerDisplay("{Operation} {TargetType}: {Status}")]
public class AlmanacChangeProposalDto
{
    public required Guid Id { get; set; }

    public required string TargetType { get; set; }

    public Guid? TargetId { get; set; }

    public required AlmanacChangeOperation Operation { get; set; }

    /// <summary>The proposed body, or null for a delete.</summary>
    public JsonNode? ProposedState { get; set; }

    public int? TargetVersion { get; set; }

    public required AlmanacProposalStatus Status { get; set; }

    public required bool AutoApproved { get; set; }

    public required Guid ProposedById { get; set; }

    public required DateTimeOffset ProposedAt { get; set; }

    public Guid? ReviewedById { get; set; }

    public DateTimeOffset? ReviewedAt { get; set; }

    public string? ReviewNote { get; set; }

    public static AlmanacChangeProposalDto From(Entities.AlmanacChangeProposal proposal) => new()
    {
        Id = proposal.Id,
        TargetType = proposal.TargetType,
        TargetId = proposal.TargetId,
        Operation = proposal.Operation,
        ProposedState = proposal.ProposedState is null ? null : JsonNode.Parse(proposal.ProposedState),
        TargetVersion = proposal.TargetVersion,
        Status = proposal.Status,
        AutoApproved = proposal.AutoApproved,
        ProposedById = proposal.ProposedById,
        ProposedAt = proposal.ProposedAt,
        ReviewedById = proposal.ReviewedById,
        ReviewedAt = proposal.ReviewedAt,
        ReviewNote = proposal.ReviewNote
    };
}
