using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using PlantKeeperAPI.Database;
using PlantKeeperAPI.DataTransferObjects;
using PlantKeeperAPI.Entities;
using PlantKeeperAPI.Enums;
using PlantKeeperAPI.Models;

namespace PlantKeeperAPI.Services;

public class PlantSpeciesService : IPlantSpeciesService
{
    private readonly PlantKeeperDbContext _dbContext;
    private readonly IMapper _mapper;

    public PlantSpeciesService(PlantKeeperDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async ValueTask<IEnumerable<PlantSpeciesDto>> ListAsync()
    {
        List<PlantSpecies> species = await WithProfiles(_dbContext.PlantSpecies.AsNoTracking())
            .OrderBy(entry => entry.Name)
            .ToListAsync();

        return _mapper.Map<IEnumerable<PlantSpeciesDto>>(species);
    }

    public async ValueTask<PlantSpeciesDto?> GetAsync(Guid speciesId)
    {
        PlantSpecies? species = await WithProfiles(_dbContext.PlantSpecies.AsNoTracking())
            .FirstOrDefaultAsync(entry => entry.Id == speciesId);

        return species is null ? null : _mapper.Map<PlantSpeciesDto>(species);
    }

    public async ValueTask<SpeciesWriteResult> CreateAsync(InputPlantSpecies input)
    {
        if (Contradicts(input)) return new SpeciesWriteResult(SpeciesWriteStatus.FloweringConflict);

        var species = _mapper.Map<PlantSpecies>(input);

        // Adding assigns the generated key, which the dependents key themselves on.
        await _dbContext.PlantSpecies.AddAsync(species);

        var care = _mapper.Map<SpeciesCareProfile>(input.Care);
        care.SpeciesId = species.Id;
        await _dbContext.SpeciesCareProfiles.AddAsync(care);

        var toxicity = _mapper.Map<SpeciesToxicityProfile>(input.Toxicity);
        toxicity.SpeciesId = species.Id;
        await _dbContext.SpeciesToxicityProfiles.AddAsync(toxicity);

        if (input.Flowering is not null)
        {
            var flowering = _mapper.Map<SpeciesFloweringProfile>(input.Flowering);
            flowering.SpeciesId = species.Id;
            await _dbContext.SpeciesFloweringProfiles.AddAsync(flowering);
        }

        await _dbContext.SaveChangesAsync();

        return new SpeciesWriteResult(SpeciesWriteStatus.Success, await GetAsync(species.Id));
    }

    public async ValueTask<SpeciesWriteResult> UpdateAsync(Guid speciesId, InputPlantSpecies input)
    {
        PlantSpecies? species = await WithProfiles(_dbContext.PlantSpecies)
            .FirstOrDefaultAsync(entry => entry.Id == speciesId);

        if (species is null) return new SpeciesWriteResult(SpeciesWriteStatus.NotFound);
        if (Contradicts(input)) return new SpeciesWriteResult(SpeciesWriteStatus.FloweringConflict);

        _mapper.Map(input, species);

        UpsertCare(species, input.Care);
        UpsertToxicity(species, input.Toxicity);
        UpsertFlowering(species, input.Flowering);

        await _dbContext.SaveChangesAsync();

        return new SpeciesWriteResult(SpeciesWriteStatus.Success, await GetAsync(speciesId));
    }

    public async ValueTask<SpeciesDeleteResult> DeleteAsync(Guid speciesId)
    {
        PlantSpecies? species = await _dbContext.PlantSpecies.FindAsync(speciesId);
        if (species is null) return new SpeciesDeleteResult(SpeciesDeleteStatus.NotFound);

        // The profile rows and matrix cells cascade - they belong to this species and mean
        // nothing without it. Plants and propagation batches do not: they belong to a
        // keeper, and the foreign key is Restrict so the database refuses instead.
        _dbContext.Remove(species);

        try
        {
            await _dbContext.SaveChangesAsync();
            return new SpeciesDeleteResult(SpeciesDeleteStatus.Success);
        }
        catch (DbUpdateException exception)
            when (exception.InnerException is PostgresException { SqlState: "23503" } violation)
        {
            _dbContext.Entry(species).State = EntityState.Unchanged;
            return new SpeciesDeleteResult(SpeciesDeleteStatus.StillInUse, violation.TableName);
        }
    }

    private static IQueryable<PlantSpecies> WithProfiles(IQueryable<PlantSpecies> query) => query
        .Include(species => species.Care)
        .Include(species => species.Toxicity)
        .Include(species => species.Flowering);

    /// <summary>A species that does not flower must not carry a flowering profile.</summary>
    private static bool Contradicts(InputPlantSpecies input) =>
        input.FloweringHabit == FloweringHabit.DoesNotFlower && input.Flowering is not null;

    private void UpsertCare(PlantSpecies species, InputSpeciesCareProfile input)
    {
        if (species.Care is not null)
        {
            _mapper.Map(input, species.Care);
            return;
        }

        var care = _mapper.Map<SpeciesCareProfile>(input);
        care.SpeciesId = species.Id;
        _dbContext.SpeciesCareProfiles.Add(care);
    }

    private void UpsertToxicity(PlantSpecies species, InputSpeciesToxicityProfile input)
    {
        if (species.Toxicity is not null)
        {
            _mapper.Map(input, species.Toxicity);
            return;
        }

        var toxicity = _mapper.Map<SpeciesToxicityProfile>(input);
        toxicity.SpeciesId = species.Id;
        _dbContext.SpeciesToxicityProfiles.Add(toxicity);
    }

    private void UpsertFlowering(PlantSpecies species, InputSpeciesFloweringProfile? input)
    {
        // Omitting the profile is how a species stops flowering.
        if (input is null)
        {
            if (species.Flowering is not null) _dbContext.SpeciesFloweringProfiles.Remove(species.Flowering);
            return;
        }

        if (species.Flowering is not null)
        {
            _mapper.Map(input, species.Flowering);
            return;
        }

        var flowering = _mapper.Map<SpeciesFloweringProfile>(input);
        flowering.SpeciesId = species.Id;
        _dbContext.SpeciesFloweringProfiles.Add(flowering);
    }
}
