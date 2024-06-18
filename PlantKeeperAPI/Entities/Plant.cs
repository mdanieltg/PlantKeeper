using System.Diagnostics;

namespace PlantKeeperAPI.Entities;

[DebuggerDisplay("Plant: {Name}")]
public class Plant
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public byte[]? Picture { get; set; }
    public string? Care { get; set; }

    public HashSet<WateringLog> WateringLogs { get; set; }
}
