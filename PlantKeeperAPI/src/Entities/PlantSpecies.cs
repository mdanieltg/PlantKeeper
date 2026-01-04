using System.Diagnostics;

namespace PlantKeeperAPI.Entities;

[DebuggerDisplay("Species: {ScientificName}")]
public class PlantSpecies
{
    public Guid Id { get; set; }
    public required string ScientificName { get; set; }
    public required string Name { get; set; }
    public required string NameInSpanish { get; set; }
    public Guid ClimateId { get; set; }
    public Guid PottingMixId { get; set; }
    public string? Comments { get; set; }

    public Climate Climate { get; init; } = null!;
    public PottingMix PottingMix { get; init; } = null!;
    public List<Plant> Plants { get; } = [];
}
