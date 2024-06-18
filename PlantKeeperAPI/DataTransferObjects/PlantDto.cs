using System.Diagnostics;

namespace PlantKeeperAPI.DataTransferObjects;

[DebuggerDisplay("Plant: {Name}")]
public class PlantDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public byte[]? Picture { get; set; }
    public string? Care { get; set; }
}
