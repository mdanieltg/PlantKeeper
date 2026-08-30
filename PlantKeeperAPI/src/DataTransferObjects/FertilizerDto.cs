using System.Diagnostics;
using PlantKeeperAPI.Enums;

namespace PlantKeeperAPI.DataTransferObjects;

[DebuggerDisplay("Fertilizer: {Name}")]
public class FertilizerDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string? NpkRatio { get; set; }
    public FertilizerCategory Category { get; set; }
}
