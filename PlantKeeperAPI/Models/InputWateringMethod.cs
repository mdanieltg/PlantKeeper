using System.Diagnostics;

namespace PlantKeeperAPI.Models;

[DebuggerDisplay("Watering method: {Name}")]
public class InputWateringMethod
{
    public required string Name { get; set; }
    public string? Description { get; set; }
}
