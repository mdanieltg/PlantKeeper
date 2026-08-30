using System.Diagnostics;

namespace PlantKeeperAPI.DataTransferObjects;

[DebuggerDisplay("Propagation method: {Name}")]
public class PropagationMethodDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
}
