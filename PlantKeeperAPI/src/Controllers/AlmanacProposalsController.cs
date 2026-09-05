using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using PlantKeeperAPI.Authorization;
using PlantKeeperAPI.DataTransferObjects;
using PlantKeeperAPI.Enums;
using PlantKeeperAPI.Models;
using PlantKeeperAPI.Services;

namespace PlantKeeperAPI.Controllers;

/// <summary>
/// The almanac's review queue, and its history - every change is recorded here, including
/// the ones that applied straight away.
/// </summary>
[ApiController]
[Route("/api/almanac-proposals")]
[Consumes(MediaTypeNames.Application.Json)]
[Produces(MediaTypeNames.Application.Json)]
[RequiresPermission(Permissions.AlmanacRead)]
public class AlmanacProposalsController : ControllerBase
{
    private readonly IAlmanacProposalService _proposals;

    public AlmanacProposalsController(IAlmanacProposalService proposals) => _proposals = proposals;

    /// <param name="status">Optional - <c>Pending</c> is the moderation queue.</param>
    [HttpGet]
    [ProducesResponseType<IEnumerable<AlmanacChangeProposalDto>>(StatusCodes.Status200OK)]
    public async ValueTask<ActionResult<IEnumerable<AlmanacChangeProposalDto>>> List(
        [FromQuery] AlmanacProposalStatus? status) =>
        Ok(await _proposals.ListAsync(status));

    [HttpGet("{proposalId:guid}")]
    [ProducesResponseType<AlmanacChangeProposalDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async ValueTask<ActionResult<AlmanacChangeProposalDto>> Get([FromRoute] Guid proposalId)
    {
        AlmanacChangeProposalDto? proposal = await _proposals.GetAsync(proposalId);
        return proposal is not null ? Ok(proposal) : NotFound();
    }

    /// <summary>Applies a queued change, unless its target moved on while it waited.</summary>
    [RequiresPermission(Permissions.AlmanacApprove)]
    [HttpPost("{proposalId:guid}/approve")]
    [ProducesResponseType<AlmanacChangeProposalDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async ValueTask<ActionResult<AlmanacChangeProposalDto>> Approve(
        [FromRoute] Guid proposalId,
        [FromBody] InputAlmanacReview review) =>
        Decide(await _proposals.ApproveAsync(proposalId, review.Note));

    [RequiresPermission(Permissions.AlmanacApprove)]
    [HttpPost("{proposalId:guid}/reject")]
    [ProducesResponseType<AlmanacChangeProposalDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async ValueTask<ActionResult<AlmanacChangeProposalDto>> Reject(
        [FromRoute] Guid proposalId,
        [FromBody] InputAlmanacReview review) =>
        Decide(await _proposals.RejectAsync(proposalId, review.Note));

    /// <summary>
    /// A refused approval is still a decision, so the proposal comes back with its new
    /// status rather than only an error - the queue is correct either way, and the reviewer
    /// can see what happened to the row they were looking at.
    /// </summary>
    private ActionResult<AlmanacChangeProposalDto> Decide(AlmanacReviewResult result) => result.Status switch
    {
        AlmanacReviewStatus.NotFound => NotFound(),

        AlmanacReviewStatus.Stale or AlmanacReviewStatus.TargetGone or AlmanacReviewStatus.AlreadyDecided =>
            Conflict(Problem(result, "Cannot be applied")),

        AlmanacReviewStatus.Invalid =>
            UnprocessableEntity(Problem(result, "Cannot be applied")),

        _ => Ok(result.Proposal)
    };

    private ProblemDetails Problem(AlmanacReviewResult result, string title)
    {
        ProblemDetails problem = new()
        {
            Status = result.Status is AlmanacReviewStatus.Invalid
                ? StatusCodes.Status422UnprocessableEntity
                : StatusCodes.Status409Conflict,
            Title = title,
            Detail = result.Detail
        };

        problem.Extensions["reason"] = result.Status.ToString();
        if (result.Proposal is not null) problem.Extensions["proposal"] = result.Proposal;

        return problem;
    }
}
