using System.Diagnostics;
using PlantKeeperAPI.Enums;

namespace PlantKeeperAPI.Entities;

/// <summary>One cell of the almanac pest-control matrix.</summary>
[DebuggerDisplay("{Safety}")]
public class SpeciesTreatmentRecommendation : IAlmanacVersioned
{
    public Guid Id { get; set; }

    /// <summary>Bumped on every saved change. See <see cref="IAlmanacVersioned" />.</summary>
    public int Version { get; set; }
    public Guid SpeciesId { get; set; }
    public Guid TreatmentId { get; set; }
    public TreatmentSafety Safety { get; set; }
    public string? Notes { get; set; }

    public PlantSpecies Species { get; init; } = null!;
    public Treatment Treatment { get; init; } = null!;
}
