using System.Diagnostics;
using PlantKeeperAPI.Enums;

namespace PlantKeeperAPI.DataTransferObjects;

[DebuggerDisplay("Pest: {Name}")]
public class PestDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public PestStatus Status { get; set; }
    public string? Notes { get; set; }
}
