using System.Diagnostics;

namespace PlantKeeperAPI.Entities;

[DebuggerDisplay("Potting mix: {Name}")]
public class PottingMix
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }

    public List<PlantSpecies> SpeciesList { get; } = [];
}
