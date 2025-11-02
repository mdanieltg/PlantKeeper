using System.Diagnostics;

namespace PlantKeeperAPI.Entities;

[DebuggerDisplay("Keeper: {Name}")]
public class Keeper
{
    public Guid Id { get; set; }
    public required string Name { get; set; }

    public HashSet<WateringLog> WateringLogs { get; set; } = new();
}
