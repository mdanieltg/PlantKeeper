using System.Diagnostics;

namespace PlantKeeperAPI.Entities;

[DebuggerDisplay("Fertilization: {Name}")]
public class Fertilizer
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }

    public List<FertilizationLog> Logs { get; } = [];
}
