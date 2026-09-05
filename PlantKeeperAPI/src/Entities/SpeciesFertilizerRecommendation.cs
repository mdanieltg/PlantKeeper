using System.Diagnostics;
using PlantKeeperAPI.Enums;

namespace PlantKeeperAPI.Entities;

/// <summary>One cell of the almanac fertilization matrix.</summary>
[DebuggerDisplay("{Category}: {Suitability}")]
public class SpeciesFertilizerRecommendation : IAlmanacVersioned
{
    public Guid Id { get; set; }

    /// <summary>Bumped on every saved change. See <see cref="IAlmanacVersioned" />.</summary>
    public int Version { get; set; }
    public Guid SpeciesId { get; set; }
    public FertilizerCategory Category { get; set; }
    public FertilizerSuitability Suitability { get; set; }
    public string? Notes { get; set; }

    public PlantSpecies Species { get; init; } = null!;
}
