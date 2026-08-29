using System.Diagnostics;
using PlantKeeperAPI.Enums;

namespace PlantKeeperAPI.Entities;

[DebuggerDisplay("Pest: {Name}")]
public class Pest
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public PestStatus Status { get; set; }
    public string? Notes { get; set; }

    public List<Treatment> Treatments { get; } = [];
    public List<BeneficialOrganism> ControlledBy { get; } = [];
}
