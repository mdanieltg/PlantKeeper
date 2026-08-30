using System.Diagnostics;
using PlantKeeperAPI.Enums;

namespace PlantKeeperAPI.DataTransferObjects;

[DebuggerDisplay("Beneficial organism: {Name}")]
public class BeneficialOrganismDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? ScientificName { get; set; }
    public BeneficialRole Role { get; set; }
    public string? SuggestedPlants { get; set; }
    public string? Notes { get; set; }
}
