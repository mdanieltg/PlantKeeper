using System.Diagnostics;

namespace PlantKeeperAPI.Entities;

[DebuggerDisplay("Propagation method: {Name}")]
public class PropagationMethod
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }

    public List<SpeciesPropagationMethod> SpeciesLinks { get; } = [];
    public List<PropagationBatch> Batches { get; } = [];
}
