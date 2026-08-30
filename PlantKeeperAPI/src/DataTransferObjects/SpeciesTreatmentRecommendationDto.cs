using System.Diagnostics;
using PlantKeeperAPI.Enums;

namespace PlantKeeperAPI.DataTransferObjects;

[DebuggerDisplay("{Safety}")]
public class SpeciesTreatmentRecommendationDto
{
    public Guid Id { get; set; }
    public Guid SpeciesId { get; set; }
    public Guid TreatmentId { get; set; }
    public TreatmentSafety Safety { get; set; }
    public string? Notes { get; set; }
}
