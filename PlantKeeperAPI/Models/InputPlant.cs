using System.Diagnostics;

namespace PlantKeeperAPI.Models;

[DebuggerDisplay("Plant: {Name}")]
public class InputPlant
{
    public required string Name { get; set; }
    public byte[]? Picture { get; set; }
    public string? Care { get; set; }
}
