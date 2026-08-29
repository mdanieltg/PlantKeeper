using System.Diagnostics;
using PlantKeeperAPI.Enums;

namespace PlantKeeperAPI.Entities;

/// <summary>One cell of the almanac pest-control matrix.</summary>
[DebuggerDisplay("{Safety}")]
public class SpeciesTreatmentRecommendation
{
    public Guid Id { get; set; }
    public Guid SpeciesId { get; set; }
    public Guid TreatmentId { get; set; }
    public TreatmentSafety Safety { get; set; }
    public string? Notes { get; set; }

    public PlantSpecies Species { get; init; } = null!;
    public Treatment Treatment { get; init; } = null!;
}
