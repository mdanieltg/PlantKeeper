using System.Diagnostics;

namespace PlantKeeperAPI.Entities;

[DebuggerDisplay("Plant: {Name}")]
public class Plant
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string NameInEnglish { get; set; }
    public Guid SoilTypeId { get; set; }
    public Guid LightingTypeId { get; set; }
    public required string HumidityConditions { get; set; }
    public string? Comments { get; set; }

    public Soil SoilType { get; init; } = null!;
    public Lighting LightingType { get; init; } = null!;
    public List<ObservationLog> ObservationLogs { get; } = [];
    public List<RepottingLog> RepottingLogs { get; } = [];
    public List<WateringLog> WateringLogs { get; } = [];
    public List<FertilizationLog> FertilizationLogs { get; } = [];
}
