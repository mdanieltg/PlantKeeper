using System.ComponentModel.DataAnnotations;
using PlantKeeperAPI.Enums;

namespace PlantKeeperAPI.Models;

/// <summary>
/// Required for every species. Blanks in the almanac are unresearched values, not absent
/// ones, so there is nothing optional here beyond the <c>*Notes</c> caveats.
/// </summary>
public class InputSpeciesCareProfile
{
    public LightLevel LightMin { get; set; }
    public LightLevel LightMax { get; set; }

    /// <summary>Qualifiers the scale cannot hold, such as "6-8h" or "aclimatar gradual".</summary>
    [StringLength(100)]
    public string? LightNotes { get; set; }

    [Range(-90, 60)]
    public int MinTemperatureCelsius { get; set; }

    [Range(-90, 60)]
    public int MaxTemperatureCelsius { get; set; }

    [StringLength(150)]
    public required string WateringRequirement { get; set; }

    [Range(0, 14)]
    public decimal SoilPhMin { get; set; }

    [Range(0, 14)]
    public decimal SoilPhMax { get; set; }

    /// <summary>Caveats the pH range cannot express, such as "tolerates up to 8.0".</summary>
    [StringLength(100)]
    public string? SoilPhNotes { get; set; }

    public WindTolerance WindTolerance { get; set; }

    [StringLength(100)]
    public string? WindToleranceNotes { get; set; }
}
