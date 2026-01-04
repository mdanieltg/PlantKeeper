using System.Diagnostics;

namespace PlantKeeperAPI.Entities;

[DebuggerDisplay("Plant: {Alias}")]
public class Plant
{
    public Guid Id { get; set; }
    public required string Alias { get; set; }
    public Guid SpeciesId { get; set; }
    public string? Comments { get; set; }

    public PlantSpecies Species { get; init; } = null!;
    public List<ObservationLog> ObservationLogs { get; } = [];
    public List<RepottingLog> RepottingLogs { get; } = [];
    public List<WateringLog> WateringLogs { get; } = [];
    public List<FertilizationLog> FertilizationLogs { get; } = [];
    public List<TreatmentLog> TreatmentLogs { get; } = [];
}
