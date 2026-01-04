using System.Diagnostics;

namespace PlantKeeperAPI.Entities;

[DebuggerDisplay("Treatment method: {Name}")]
public class TreatmentMethod
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }

    public List<TreatmentLog> TreatmentLogs { get; } = [];
}
