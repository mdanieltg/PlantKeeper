using System.Diagnostics;
using PlantKeeperAPI.Enums;

namespace PlantKeeperAPI.Entities;

[DebuggerDisplay("Species: {ScientificName}")]
public class PlantSpecies
{
    public Guid Id { get; set; }
    public required string ScientificName { get; set; }

    /// <summary>Primary common name, in whichever language the collection uses for it.</summary>
    public required string Name { get; set; }

    /// <summary>English equivalent where one exists and differs from <see cref="Name" />.</summary>
    public string? NameInEnglish { get; set; }

    public Guid ClimateId { get; set; }
    public Guid PottingMixId { get; set; }

    /// <summary>
    /// Whether the species flowers in cultivation. <c>DoesNotFlower</c> implies
    /// <see cref="Flowering" /> is null.
    /// </summary>
    public FloweringHabit FloweringHabit { get; set; }

    public required string FertilizationFrequency { get; set; }
    public string? FertilizationNotes { get; set; }
    public string? Comments { get; set; }

    public Climate Climate { get; init; } = null!;
    public PottingMix PottingMix { get; init; } = null!;

    public SpeciesCareProfile Care { get; init; } = null!;
    public SpeciesToxicityProfile Toxicity { get; init; } = null!;
    public SpeciesFloweringProfile? Flowering { get; init; }

    public List<Plant> Plants { get; } = [];
    public List<BeneficialOrganism> BeneficialOrganisms { get; } = [];
    public List<SpeciesFertilizerRecommendation> FertilizerRecommendations { get; } = [];
    public List<SpeciesTreatmentRecommendation> TreatmentRecommendations { get; } = [];
    public List<SpeciesPropagationMethod> PropagationMethods { get; } = [];
    public List<PropagationBatch> PropagationBatches { get; } = [];
}
