using System.Diagnostics;

namespace PlantKeeperAPI.DataTransferObjects;

[DebuggerDisplay("Potting mix: {Name}")]
public class PottingMixDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
}
