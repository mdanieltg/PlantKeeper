using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using PlantKeeperAPI.Enums;

namespace PlantKeeperAPI.Models;

[DebuggerDisplay("Beneficial organism: {Name}")]
public class InputBeneficialOrganism
{
    [StringLength(50)]
    public required string Name { get; set; }

    [StringLength(100)]
    public string? ScientificName { get; set; }

    public BeneficialRole Role { get; set; }

    /// <summary>Locally available plants that attract it but are not in the collection.</summary>
    [StringLength(255)]
    public string? SuggestedPlants { get; set; }

    [StringLength(300)]
    public string? Notes { get; set; }
}
