using PlantKeeperAPI.DataTransferObjects;
using PlantKeeperAPI.Enums;

namespace PlantKeeperAPI.Services;

public enum AlmanacReviewStatus
{
    Applied,
    NotFound,

    /// <summary>Already applied or rejected. A decision is made once.</summary>
    AlreadyDecided,

    /// <summary>
    /// The target moved on after the proposal was written. Refused rather than applied over
    /// the top of whatever changed it.
    /// </summary>
    Stale,

    /// <summary>The target was deleted while the proposal waited.</summary>
    TargetGone,

    /// <summary>Applying it would break a constraint - usually a reference that no longer resolves.</summary>
    Invalid,

    Rejected
}

/// <param name="Detail">Why, when the status is not <see cref="AlmanacReviewStatus.Applied" />.</param>
public record AlmanacReviewResult(
    AlmanacReviewStatus Status,
    AlmanacChangeProposalDto? Proposal = null,
    string? Detail = null);

/// <summary>
/// The shared almanac's change-control. Every almanac write goes through here, so the
/// proposal table is both the review queue and the history of what has been changed.
/// <para>
/// A keeper holding <c>almanac.approve</c> never waits, and neither does anyone creating a
/// standalone row: it references nothing yet, so it cannot invalidate anyone's plants. A
/// <em>nested</em> create is not standalone - adding a recommendation or a profile to a
/// species other keepers already grow edits that species - so it queues like any edit.
/// Everything that applies is recorded as auto-approved; everything else waits.
/// </para>
/// </summary>
public interface IAlmanacProposalService
{
    /// <summary>
    /// Records the intent to change the almanac, and says whether the caller may go ahead.
    /// <para>
    /// Returns <c>null</c> when the caller may apply the change themselves - the proposal
    /// has already been written as applied and auto-approved. Returns the queued proposal
    /// when they may not, in which case the caller must not touch the almanac.
    /// </para>
    /// </summary>
    /// <param name="targetVersion">
    /// The target's current version, for an update or delete. Null for a create.
    /// </param>
    ValueTask<AlmanacChangeProposalDto?> SubmitAsync(
        string targetType,
        AlmanacChangeOperation operation,
        object? proposedState,
        Guid? targetId = null,
        int? targetVersion = null);

    ValueTask<IEnumerable<AlmanacChangeProposalDto>> ListAsync(AlmanacProposalStatus? status);

    ValueTask<AlmanacChangeProposalDto?> GetAsync(Guid proposalId);

    /// <summary>Applies a pending proposal, unless its target has moved on since.</summary>
    ValueTask<AlmanacReviewResult> ApproveAsync(Guid proposalId, string? note);

    ValueTask<AlmanacReviewResult> RejectAsync(Guid proposalId, string? note);
}
