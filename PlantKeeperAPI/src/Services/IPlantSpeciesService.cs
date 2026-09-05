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

public enum SpeciesDeleteStatus
{
    Success,
    NotFound,

    /// <summary>
    /// Plants or propagation batches still point at it. Since <c>RestrictAlmanacDeletes</c>
    /// the database refuses this rather than cascading through collections the deleter
    /// cannot see.
    /// </summary>
    StillInUse
}

/// <param name="ReferencedBy">
/// The table still holding a reference, when <see cref="SpeciesDeleteStatus.StillInUse" />.
/// Null otherwise.
/// </param>
public record SpeciesDeleteResult(SpeciesDeleteStatus Status, string? ReferencedBy = null);

/// <summary>
/// The species aggregate. Care and toxicity profiles are required in C# but the foreign
/// key sits on the dependent, so the database cannot enforce their presence - writing the whole
/// aggregate in one place is what keeps that guarantee real.
/// </summary>
public interface IPlantSpeciesService
{
    ValueTask<IEnumerable<PlantSpeciesDto>> ListAsync();
    ValueTask<PlantSpeciesDto?> GetAsync(Guid speciesId);
    ValueTask<SpeciesWriteResult> CreateAsync(InputPlantSpecies input);
    ValueTask<SpeciesWriteResult> UpdateAsync(Guid speciesId, InputPlantSpecies input);
    ValueTask<SpeciesDeleteResult> DeleteAsync(Guid speciesId);
}
