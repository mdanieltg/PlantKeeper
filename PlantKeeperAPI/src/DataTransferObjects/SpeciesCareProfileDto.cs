using System.Diagnostics;
using PlantKeeperAPI.Enums;

namespace PlantKeeperAPI.DataTransferObjects;

[DebuggerDisplay("Care profile: {LightMin}..{LightMax}")]
public class SpeciesCareProfileDto
{
    public Guid SpeciesId { get; set; }
    public LightLevel LightMin { get; set; }
    public LightLevel LightMax { get; set; }
    public string? LightNotes { get; set; }
    public int MinTemperatureCelsius { get; set; }
    public int MaxTemperatureCelsius { get; set; }
    public required string WateringRequirement { get; set; }
    public decimal SoilPhMin { get; set; }
    public decimal SoilPhMax { get; set; }
    public string? SoilPhNotes { get; set; }
    public WindTolerance WindTolerance { get; set; }
    public string? WindToleranceNotes { get; set; }
}
