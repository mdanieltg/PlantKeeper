using System.Diagnostics;
using PlantKeeperAPI.Enums;

namespace PlantKeeperAPI.Entities;

[DebuggerDisplay("Pest: {Name}")]
public class Pest : IAlmanacVersioned
{
    public Guid Id { get; set; }

    /// <summary>Bumped on every saved change. See <see cref="IAlmanacVersioned" />.</summary>
    public int Version { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public PestStatus Status { get; set; }
    public string? Notes { get; set; }

    public List<Treatment> Treatments { get; } = [];
    public List<BeneficialOrganism> ControlledBy { get; } = [];
}
