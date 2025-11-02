using System.Diagnostics;

namespace PlantKeeperAPI.Entities;

[DebuggerDisplay("Watering method: {Name}")]
public class WateringMethod
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }

    public HashSet<WateringLog> WateringLogs { get; set; } = new();
}
