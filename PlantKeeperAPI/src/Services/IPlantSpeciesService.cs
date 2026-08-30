using PlantKeeperAPI.DataTransferObjects;
using PlantKeeperAPI.Models;

namespace PlantKeeperAPI.Services;

public enum SpeciesWriteStatus
{
    Success,
    NotFound,

    /// <summary>A flowering profile was supplied for a species that does not flower.</summary>
    FloweringConflict
}

public record SpeciesWriteResult(SpeciesWriteStatus Status, PlantSpeciesDto? Species = null);

/// <summary>
/// The species aggregate. Care and toxicity profiles are required in C# but the foreign
/// key sits on the dependent, so MySQL cannot enforce their presence - writing the whole
/// aggregate in one place is what keeps that guarantee real.
/// </summary>
public interface IPlantSpeciesService
{
    ValueTask<IEnumerable<PlantSpeciesDto>> ListAsync();
    ValueTask<PlantSpeciesDto?> GetAsync(Guid speciesId);
    ValueTask<SpeciesWriteResult> CreateAsync(InputPlantSpecies input);
    ValueTask<SpeciesWriteResult> UpdateAsync(Guid speciesId, InputPlantSpecies input);
    ValueTask<bool> DeleteAsync(Guid speciesId);
}
