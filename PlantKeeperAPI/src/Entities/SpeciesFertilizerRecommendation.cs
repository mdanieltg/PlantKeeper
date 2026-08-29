using System.Diagnostics;
using PlantKeeperAPI.Enums;

namespace PlantKeeperAPI.Entities;

/// <summary>One cell of the almanac fertilization matrix.</summary>
[DebuggerDisplay("{Category}: {Suitability}")]
public class SpeciesFertilizerRecommendation
{
    public Guid Id { get; set; }
    public Guid SpeciesId { get; set; }
    public FertilizerCategory Category { get; set; }
    public FertilizerSuitability Suitability { get; set; }
    public string? Notes { get; set; }

    public PlantSpecies Species { get; init; } = null!;
}
