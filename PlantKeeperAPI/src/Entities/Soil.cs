using System.Diagnostics;

namespace PlantKeeperAPI.Entities;

[DebuggerDisplay("Soil: {Type}")]
public class Soil
{
    public Guid Id { get; set; }
    public required string Type { get; set; }
    public string? Description { get; set; }

    public List<Plant> Plants { get; set; } = [];
}
