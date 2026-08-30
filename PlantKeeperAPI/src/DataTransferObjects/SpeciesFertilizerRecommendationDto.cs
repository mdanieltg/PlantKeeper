using System.Diagnostics;
using PlantKeeperAPI.Enums;

namespace PlantKeeperAPI.DataTransferObjects;

[DebuggerDisplay("{Category}: {Suitability}")]
public class SpeciesFertilizerRecommendationDto
{
    public Guid Id { get; set; }
    public Guid SpeciesId { get; set; }
    public FertilizerCategory Category { get; set; }
    public FertilizerSuitability Suitability { get; set; }
    public string? Notes { get; set; }
}
