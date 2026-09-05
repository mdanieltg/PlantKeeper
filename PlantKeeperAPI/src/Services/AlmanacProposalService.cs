using System.Text.Json;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;
using PlantKeeperAPI.Authorization;
using PlantKeeperAPI.Database;
using PlantKeeperAPI.DataTransferObjects;
using PlantKeeperAPI.Entities;
using PlantKeeperAPI.Enums;
using PlantKeeperAPI.Extensions;
using PlantKeeperAPI.Models;

namespace PlantKeeperAPI.Services;

/// <inheritdoc />
public partial class AlmanacProposalService : IAlmanacProposalService
{
    /// <summary>PostgreSQL <c>foreign_key_violation</c>.</summary>
    private const string ForeignKeyViolation = "23503";

    /// <summary>
    /// The wire shape, so a body stored here round-trips exactly as the endpoint received
    /// it - camelCase names and string enums. Anything else and a proposal would apply
    /// differently from the request that wrote it.
    /// </summary>
    private static readonly JsonSerializerOptions Json =
        new JsonSerializerOptions(JsonSerializerDefaults.Web).ApplyPlantKeeperDefaults();

    private readonly ICurrentKeeper _currentKeeper;
    private readonly PlantKeeperDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly IPlantSpeciesService _species;

    public AlmanacProposalService(
        PlantKeeperDbContext dbContext,
        IMapper mapper,
        ICurrentKeeper currentKeeper,
        IPlantSpeciesService species)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _currentKeeper = currentKeeper;
        _species = species;
    }

    public async ValueTask<AlmanacChangeProposalDto?> SubmitAsync(
        string targetType,
        AlmanacChangeOperation operation,
        object? proposedState,
        Guid? targetId = null,
        int? targetVersion = null)
    {
        // A standalone create references nothing yet, so it cannot invalidate anyone's
        // plants and needs no review. Everything else does, unless the proposer could have
        // approved it anyway.
        //
        // "Standalone" is the load-bearing word, and targetId is what says so. A create
        // carrying one is nested - a recommendation on a species, a profile under it - and
        // adding content to a species other keepers already grow is an edit of that
        // species, whatever the HTTP verb says. Only a create with no parent skips review.
        bool mayApply = (operation == AlmanacChangeOperation.Create && targetId is null)
                        || _currentKeeper.HasPermission(Permissions.AlmanacApprove);

        AlmanacChangeProposal proposal = new()
        {
            TargetType = targetType,
            TargetId = targetId,
            Operation = operation,
            ProposedState = proposedState is null ? null : JsonSerializer.Serialize(proposedState, Json),
            TargetVersion = targetVersion,
            Status = mayApply ? AlmanacProposalStatus.Applied : AlmanacProposalStatus.Pending,
            AutoApproved = mayApply,
            ProposedById = _currentKeeper.Id,
            ProposedAt = DateTimeOffset.UtcNow,
            ReviewedById = mayApply ? _currentKeeper.Id : null,
            ReviewedAt = mayApply ? DateTimeOffset.UtcNow : null
        };

        await _dbContext.AlmanacChangeProposals.AddAsync(proposal);

        if (mayApply)
        {
            // Deliberately not saved here. The caller applies the change and saves, and this
            // row goes in the same SaveChanges - so a write that fails validation or a
            // foreign key leaves no record claiming it succeeded.
            return null;
        }

        await _dbContext.SaveChangesAsync();
        return AlmanacChangeProposalDto.From(proposal);
    }

    public async ValueTask<IEnumerable<AlmanacChangeProposalDto>> ListAsync(AlmanacProposalStatus? status)
    {
        IQueryable<AlmanacChangeProposal> query = _dbContext.AlmanacChangeProposals.AsNoTracking();
        if (status is not null) query = query.Where(proposal => proposal.Status == status);

        List<AlmanacChangeProposal> proposals = await query
            .OrderBy(proposal => proposal.Status == AlmanacProposalStatus.Pending ? 0 : 1)
            .ThenByDescending(proposal => proposal.ProposedAt)
            .ToListAsync();

        return proposals.Select(AlmanacChangeProposalDto.From);
    }

    public async ValueTask<AlmanacChangeProposalDto?> GetAsync(Guid proposalId)
    {
        AlmanacChangeProposal? proposal = await _dbContext.AlmanacChangeProposals
            .AsNoTracking()
            .FirstOrDefaultAsync(row => row.Id == proposalId);

        return proposal is null ? null : AlmanacChangeProposalDto.From(proposal);
    }

    public async ValueTask<AlmanacReviewResult> ApproveAsync(Guid proposalId, string? note)
    {
        AlmanacChangeProposal? proposal = await _dbContext.AlmanacChangeProposals
            .FirstOrDefaultAsync(row => row.Id == proposalId);

        if (proposal is null) return new AlmanacReviewResult(AlmanacReviewStatus.NotFound);

        if (proposal.Status != AlmanacProposalStatus.Pending)
            return new AlmanacReviewResult(AlmanacReviewStatus.AlreadyDecided,
                AlmanacChangeProposalDto.From(proposal),
                $"This proposal was already {proposal.Status.ToString().ToLowerInvariant()}.");

        // Staleness first. Applying over a row that moved on would overwrite whatever changed
        // it with a body written against a version nobody is looking at any more.
        int? currentVersion = await CurrentVersionAsync(proposal);

        if (proposal.TargetVersion is not null && currentVersion is null)
            return await RefuseAsync(proposal, AlmanacReviewStatus.TargetGone,
                "The target no longer exists.", note);

        if (proposal.TargetVersion is not null && currentVersion != proposal.TargetVersion)
            return await RefuseAsync(proposal, AlmanacReviewStatus.Stale,
                $"The target has changed since this was written (version {proposal.TargetVersion} " +
                $"then, {currentVersion} now).", note);

        // One transaction around the change and the decision. PlantSpeciesService saves on
        // its own, so without this an approved species edit could land while the proposal
        // stayed pending - and be applied a second time by the next reviewer.
        await using IDbContextTransaction transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            if (!await ApplyAsync(proposal))
            {
                await transaction.RollbackAsync();
                return await RefuseAsync(proposal, AlmanacReviewStatus.TargetGone,
                    "The target no longer exists.", note);
            }

            proposal.Status = AlmanacProposalStatus.Applied;
            proposal.ReviewedById = _currentKeeper.Id;
            proposal.ReviewedAt = DateTimeOffset.UtcNow;
            proposal.ReviewNote = note;

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (DbUpdateException exception)
            when (exception.InnerException is PostgresException { SqlState: ForeignKeyViolation })
        {
            await transaction.RollbackAsync();
            return await RefuseAsync(proposal, AlmanacReviewStatus.Invalid,
                "Applying this would break a reference that no longer resolves.", note);
        }

        return new AlmanacReviewResult(AlmanacReviewStatus.Applied, AlmanacChangeProposalDto.From(proposal));
    }

    public async ValueTask<AlmanacReviewResult> RejectAsync(Guid proposalId, string? note)
    {
        AlmanacChangeProposal? proposal = await _dbContext.AlmanacChangeProposals
            .FirstOrDefaultAsync(row => row.Id == proposalId);

        if (proposal is null) return new AlmanacReviewResult(AlmanacReviewStatus.NotFound);

        if (proposal.Status != AlmanacProposalStatus.Pending)
            return new AlmanacReviewResult(AlmanacReviewStatus.AlreadyDecided,
                AlmanacChangeProposalDto.From(proposal),
                $"This proposal was already {proposal.Status.ToString().ToLowerInvariant()}.");

        return await RefuseAsync(proposal, AlmanacReviewStatus.Rejected, note, note);
    }

    /// <summary>Records a refusal and saves it. The almanac itself is left alone.</summary>
    private async ValueTask<AlmanacReviewResult> RefuseAsync(
        AlmanacChangeProposal proposal,
        AlmanacReviewStatus status,
        string? detail,
        string? note)
    {
        // Anything the failed apply attempt tracked must not reach the database - only the
        // decision on the proposal itself.
        foreach (var entry in _dbContext.ChangeTracker.Entries().ToArray())
            if (!ReferenceEquals(entry.Entity, proposal))
                entry.State = EntityState.Detached;

        _dbContext.Attach(proposal);
        proposal.Status = AlmanacProposalStatus.Rejected;
        proposal.ReviewedById = _currentKeeper.Id;
        proposal.ReviewedAt = DateTimeOffset.UtcNow;
        proposal.ReviewNote = note ?? detail;

        await _dbContext.SaveChangesAsync();

        return new AlmanacReviewResult(status, AlmanacChangeProposalDto.From(proposal), detail);
    }
}
