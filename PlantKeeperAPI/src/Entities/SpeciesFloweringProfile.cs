using System.Diagnostics;

namespace PlantKeeperAPI.Entities;

/// <summary>
/// Flowering and seed-collection detail. Optional as a whole — a species that does not
/// flower simply has no row, which is why every member here is required: a profile that
/// exists is complete.
/// </summary>
[DebuggerDisplay("Flowering: {BloomSeason}")]
public class SpeciesFloweringProfile : IAlmanacVersioned
{
    /// <summary>Primary key and foreign key both — one profile per species.</summary>
    public Guid SpeciesId { get; set; }

    /// <summary>Bumped on every saved change. See <see cref="IAlmanacVersioned" />.</summary>
    public int Version { get; set; }

    public required string BloomSeason { get; set; }
    public required string BloomCareNotes { get; set; }
    public required string SeedViability { get; set; }
    public required string SeedHarvestTiming { get; set; }

    public PlantSpecies Species { get; init; } = null!;
}
