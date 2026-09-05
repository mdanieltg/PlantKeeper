using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PlantKeeperAPI.Entities;
using PlantKeeperAPI.Enums;
using PlantKeeperAPI.Models;

namespace PlantKeeperAPI.Services;

/// <summary>
/// Turning a stored proposal back into a change.
/// <para>
/// An explicit switch rather than a registry of generic handlers. Eighteen targets in three
/// shapes - plain rows, the species aggregate, and the payload-free sets - and one line per
/// target that a reader can grep for. A reflection-driven version would be shorter and much
/// harder to be sure of.
/// </para>
/// </summary>
public partial class AlmanacProposalService
{
    /// <summary>Applies the proposal. False means the target was not there to change.</summary>
    private ValueTask<bool> ApplyAsync(AlmanacChangeProposal proposal) => proposal.TargetType switch
    {
        AlmanacTargets.Climate => ApplyRowAsync<Climate, InputClimate>(proposal),
        AlmanacTargets.PottingMix => ApplyRowAsync<PottingMix, InputPottingMix>(proposal),
        AlmanacTargets.WateringMethod => ApplyRowAsync<WateringMethod, InputWateringMethod>(proposal),
        AlmanacTargets.Fertilizer => ApplyRowAsync<Fertilizer, InputFertilizer>(proposal),
        AlmanacTargets.Treatment => ApplyRowAsync<Treatment, InputTreatment>(proposal),
        AlmanacTargets.PropagationMethod => ApplyRowAsync<PropagationMethod, InputPropagationMethod>(proposal),
        AlmanacTargets.Pest => ApplyRowAsync<Pest, InputPest>(proposal),
        AlmanacTargets.BeneficialOrganism => ApplyRowAsync<BeneficialOrganism, InputBeneficialOrganism>(proposal),

        AlmanacTargets.PlantSpecies => ApplySpeciesAsync(proposal),

        AlmanacTargets.SpeciesCareProfile =>
            ApplyProfileAsync<SpeciesCareProfile, InputSpeciesCareProfile>(proposal),
        AlmanacTargets.SpeciesToxicityProfile =>
            ApplyProfileAsync<SpeciesToxicityProfile, InputSpeciesToxicityProfile>(proposal),
        AlmanacTargets.SpeciesFloweringProfile =>
            ApplyProfileAsync<SpeciesFloweringProfile, InputSpeciesFloweringProfile>(proposal),

        AlmanacTargets.SpeciesFertilizerRecommendation =>
            ApplyMatrixAsync<SpeciesFertilizerRecommendation, InputSpeciesFertilizerRecommendation>(proposal),
        AlmanacTargets.SpeciesTreatmentRecommendation =>
            ApplyMatrixAsync<SpeciesTreatmentRecommendation, InputSpeciesTreatmentRecommendation>(proposal),
        AlmanacTargets.SpeciesPropagationMethod =>
            ApplyMatrixAsync<SpeciesPropagationMethod, InputSpeciesPropagationMethod>(proposal),

        AlmanacTargets.PestTreatments => ApplyPestTreatmentsAsync(proposal),
        AlmanacTargets.OrganismSpecies => ApplyOrganismSpeciesAsync(proposal),
        AlmanacTargets.OrganismPests => ApplyOrganismPestsAsync(proposal),

        // Unreachable from the endpoints, which all pass an AlmanacTargets constant. Loud
        // rather than silent, because a proposal nobody can apply would sit pending forever.
        _ => throw new InvalidOperationException($"No applier for almanac target '{proposal.TargetType}'.")
    };

    /// <summary>The target's version now, or null if it is gone. Link sets take their parent's.</summary>
    private async ValueTask<int?> CurrentVersionAsync(AlmanacChangeProposal proposal)
    {
        if (proposal.TargetId is not { } targetId) return null;

        return proposal.TargetType switch
        {
            AlmanacTargets.Climate => await VersionOfAsync<Climate>(targetId),
            AlmanacTargets.PottingMix => await VersionOfAsync<PottingMix>(targetId),
            AlmanacTargets.WateringMethod => await VersionOfAsync<WateringMethod>(targetId),
            AlmanacTargets.Fertilizer => await VersionOfAsync<Fertilizer>(targetId),
            AlmanacTargets.Treatment => await VersionOfAsync<Treatment>(targetId),
            AlmanacTargets.PropagationMethod => await VersionOfAsync<PropagationMethod>(targetId),
            AlmanacTargets.PlantSpecies => await VersionOfAsync<PlantSpecies>(targetId),

            AlmanacTargets.Pest or AlmanacTargets.PestTreatments => await VersionOfAsync<Pest>(targetId),
            AlmanacTargets.BeneficialOrganism or AlmanacTargets.OrganismSpecies or AlmanacTargets.OrganismPests =>
                await VersionOfAsync<BeneficialOrganism>(targetId),

            AlmanacTargets.SpeciesCareProfile => await ProfileVersionAsync<SpeciesCareProfile>(targetId),
            AlmanacTargets.SpeciesToxicityProfile => await ProfileVersionAsync<SpeciesToxicityProfile>(targetId),
            AlmanacTargets.SpeciesFloweringProfile => await ProfileVersionAsync<SpeciesFloweringProfile>(targetId),

            AlmanacTargets.SpeciesFertilizerRecommendation =>
                await VersionOfAsync<SpeciesFertilizerRecommendation>(targetId),
            AlmanacTargets.SpeciesTreatmentRecommendation =>
                await VersionOfAsync<SpeciesTreatmentRecommendation>(targetId),
            AlmanacTargets.SpeciesPropagationMethod =>
                await VersionOfAsync<SpeciesPropagationMethod>(targetId),

            _ => null
        };
    }

    private async ValueTask<int?> VersionOfAsync<TEntity>(Guid id) where TEntity : class, IAlmanacVersioned =>
        await _dbContext.Set<TEntity>()
            .AsNoTracking()
            .Where(row => EF.Property<Guid>(row, "Id") == id)
            .Select(row => (int?)row.Version)
            .FirstOrDefaultAsync();

    /// <summary>Profiles are keyed by their species, so their id column is <c>SpeciesId</c>.</summary>
    private async ValueTask<int?> ProfileVersionAsync<TEntity>(Guid speciesId)
        where TEntity : class, IAlmanacVersioned =>
        await _dbContext.Set<TEntity>()
            .AsNoTracking()
            .Where(row => EF.Property<Guid>(row, "SpeciesId") == speciesId)
            .Select(row => (int?)row.Version)
            .FirstOrDefaultAsync();

    private TInput Read<TInput>(AlmanacChangeProposal proposal) =>
        JsonSerializer.Deserialize<TInput>(proposal.ProposedState
                                           ?? throw new InvalidOperationException(
                                               $"Proposal {proposal.Id} carries no state to apply."), Json)
        ?? throw new InvalidOperationException($"Proposal {proposal.Id} holds unreadable state.");

    // ---- plain rows -------------------------------------------------------------------

    private async ValueTask<bool> ApplyRowAsync<TEntity, TInput>(AlmanacChangeProposal proposal)
        where TEntity : class, IAlmanacVersioned
    {
        if (proposal.Operation is AlmanacChangeOperation.Create)
        {
            await _dbContext.Set<TEntity>().AddAsync(_mapper.Map<TEntity>(Read<TInput>(proposal)));
            return true;
        }

        TEntity? row = await _dbContext.Set<TEntity>()
            .FirstOrDefaultAsync(entry => EF.Property<Guid>(entry, "Id") == proposal.TargetId);

        if (row is null) return false;

        if (proposal.Operation is AlmanacChangeOperation.Delete) _dbContext.Remove(row);
        else _mapper.Map(Read<TInput>(proposal), row);

        return true;
    }

    // ---- the species aggregate --------------------------------------------------------

    private async ValueTask<bool> ApplySpeciesAsync(AlmanacChangeProposal proposal)
    {
        switch (proposal.Operation)
        {
            case AlmanacChangeOperation.Create:
                SpeciesWriteResult created = await _species.CreateAsync(Read<InputPlantSpecies>(proposal));
                return created.Status is SpeciesWriteStatus.Success;

            case AlmanacChangeOperation.Update:
                SpeciesWriteResult updated =
                    await _species.UpdateAsync(proposal.TargetId!.Value, Read<InputPlantSpecies>(proposal));
                return updated.Status is SpeciesWriteStatus.Success;

            default:
                SpeciesDeleteResult deleted = await _species.DeleteAsync(proposal.TargetId!.Value);
                return deleted.Status is SpeciesDeleteStatus.Success;
        }
    }

    // ---- the three 1:1 profiles, keyed by species -------------------------------------

    private async ValueTask<bool> ApplyProfileAsync<TEntity, TInput>(AlmanacChangeProposal proposal)
        where TEntity : class, IAlmanacVersioned
    {
        Guid speciesId = proposal.TargetId!.Value;

        TEntity? profile = await _dbContext.Set<TEntity>()
            .FirstOrDefaultAsync(row => EF.Property<Guid>(row, "SpeciesId") == speciesId);

        if (proposal.Operation is AlmanacChangeOperation.Delete)
        {
            if (profile is null) return false;

            _dbContext.Remove(profile);
            return true;
        }

        // PUT upserts here as it does on the endpoint, so an approved proposal behaves the
        // same whether or not the profile appeared while it waited.
        if (profile is null)
        {
            profile = _mapper.Map<TEntity>(Read<TInput>(proposal));
            _dbContext.Entry(profile).Property("SpeciesId").CurrentValue = speciesId;
            await _dbContext.Set<TEntity>().AddAsync(profile);
        }
        else
        {
            _mapper.Map(Read<TInput>(proposal), profile);
        }

        return await _dbContext.PlantSpecies.AnyAsync(species => species.Id == speciesId);
    }

    // ---- the three matrices: own id, but created under a species ----------------------

    private async ValueTask<bool> ApplyMatrixAsync<TEntity, TInput>(AlmanacChangeProposal proposal)
        where TEntity : class, IAlmanacVersioned
    {
        if (proposal.Operation is AlmanacChangeOperation.Create)
        {
            // TargetId is the parent species on a nested create - the row itself has no id yet.
            TEntity row = _mapper.Map<TEntity>(Read<TInput>(proposal));
            _dbContext.Entry(row).Property("SpeciesId").CurrentValue = proposal.TargetId!.Value;
            await _dbContext.Set<TEntity>().AddAsync(row);

            return await _dbContext.PlantSpecies.AnyAsync(species => species.Id == proposal.TargetId);
        }

        return await ApplyRowAsync<TEntity, TInput>(proposal);
    }

    // ---- the payload-free sets, replaced wholesale ------------------------------------

    private async ValueTask<bool> ApplyPestTreatmentsAsync(AlmanacChangeProposal proposal)
    {
        Pest? pest = await _dbContext.Pests
            .Include(entry => entry.Treatments)
            .FirstOrDefaultAsync(entry => entry.Id == proposal.TargetId);

        if (pest is null) return false;

        Guid[] ids = Read<Guid[]>(proposal);
        List<Treatment> treatments = await _dbContext.Treatments
            .Where(treatment => ids.Contains(treatment.Id))
            .ToListAsync();

        if (treatments.Count != ids.Distinct().Count()) return false;

        pest.Treatments.Clear();
        pest.Treatments.AddRange(treatments);
        return true;
    }

    private async ValueTask<bool> ApplyOrganismSpeciesAsync(AlmanacChangeProposal proposal)
    {
        BeneficialOrganism? organism = await _dbContext.BeneficialOrganisms
            .Include(entry => entry.SupportingSpecies)
            .FirstOrDefaultAsync(entry => entry.Id == proposal.TargetId);

        if (organism is null) return false;

        Guid[] ids = Read<Guid[]>(proposal);
        List<PlantSpecies> species = await _dbContext.PlantSpecies
            .Where(entry => ids.Contains(entry.Id))
            .ToListAsync();

        if (species.Count != ids.Distinct().Count()) return false;

        organism.SupportingSpecies.Clear();
        organism.SupportingSpecies.AddRange(species);
        return true;
    }

    private async ValueTask<bool> ApplyOrganismPestsAsync(AlmanacChangeProposal proposal)
    {
        BeneficialOrganism? organism = await _dbContext.BeneficialOrganisms
            .Include(entry => entry.PestsControlled)
            .FirstOrDefaultAsync(entry => entry.Id == proposal.TargetId);

        if (organism is null) return false;

        Guid[] ids = Read<Guid[]>(proposal);
        List<Pest> pests = await _dbContext.Pests
            .Where(entry => ids.Contains(entry.Id))
            .ToListAsync();

        if (pests.Count != ids.Distinct().Count()) return false;

        organism.PestsControlled.Clear();
        organism.PestsControlled.AddRange(pests);
        return true;
    }
}
