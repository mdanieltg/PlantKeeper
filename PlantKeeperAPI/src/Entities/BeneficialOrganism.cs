using System.Diagnostics;
using PlantKeeperAPI.Enums;

namespace PlantKeeperAPI.Entities;

[DebuggerDisplay("Beneficial organism: {Name}")]
public class BeneficialOrganism
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? ScientificName { get; set; }
    public BeneficialRole Role { get; set; }

    /// <summary>
    /// Plants that are easy to source locally and would attract this organism, but are
    /// not part of the collection, so they have no <see cref="PlantSpecies" /> row.
    /// </summary>
    public string? SuggestedPlants { get; set; }

    public string? Notes { get; set; }

    public List<PlantSpecies> SupportingSpecies { get; } = [];
    public List<Pest> PestsControlled { get; } = [];
}
