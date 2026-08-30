using System.Diagnostics;
using PlantKeeperAPI.Enums;

namespace PlantKeeperAPI.DataTransferObjects;

[DebuggerDisplay("Toxicity: humans {ToHumans}, pets {ToPets}")]
public class SpeciesToxicityProfileDto
{
    public Guid SpeciesId { get; set; }
    public Toxicity ToHumans { get; set; }
    public string? ToHumansNotes { get; set; }
    public Toxicity ToPets { get; set; }
    public string? ToPetsNotes { get; set; }
}
