using System.Diagnostics;
using PlantKeeperAPI.Enums;

namespace PlantKeeperAPI.Entities;

/// <summary>
/// Growing requirements for a species. Required: a species without a care profile is
/// one nobody has researched, and the schema should say so rather than hide it behind
/// nulls.
/// </summary>
[DebuggerDisplay("Care profile: {LightMin}..{LightMax}, {MinTemperatureCelsius}-{MaxTemperatureCelsius}C")]
public class SpeciesCareProfile
{
    /// <summary>Primary key and foreign key both — one profile per species.</summary>
    public Guid SpeciesId { get; set; }

    public LightLevel LightMin { get; set; }
    public LightLevel LightMax { get; set; }

    /// <summary>Qualifiers the scale cannot hold, such as "6-8h" or "aclimatar gradual".</summary>
    public string? LightNotes { get; set; }

    public int MinTemperatureCelsius { get; set; }
    public int MaxTemperatureCelsius { get; set; }

    public required string WateringRequirement { get; set; }

    public decimal SoilPhMin { get; set; }
    public decimal SoilPhMax { get; set; }

    /// <summary>Caveats the pH range cannot express, such as "tolerates up to 8.0".</summary>
    public string? SoilPhNotes { get; set; }

    public WindTolerance WindTolerance { get; set; }
    public string? WindToleranceNotes { get; set; }

    public PlantSpecies Species { get; init; } = null!;
}
