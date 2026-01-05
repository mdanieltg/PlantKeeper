using System.Diagnostics;

namespace PlantKeeperAPI.Entities;

[DebuggerDisplay("Treatment: {Name}")]
public class Treatment
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }

    public List<TreatmentLog> Logs { get; } = [];
}
