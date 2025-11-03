using System.Diagnostics;

namespace PlantKeeperAPI.Entities;

[DebuggerDisplay("Fertilization method: {Name}")]
public class FertilizationMethod
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }

    public List<FertilizationLog> FertilizationLogs { get; } = [];
}
