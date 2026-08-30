using System.Diagnostics;
using PlantKeeperAPI.Enums;

namespace PlantKeeperAPI.DataTransferObjects;

[DebuggerDisplay("Species: {ScientificName}")]
public class PlantSpeciesDto
{
    public Guid Id { get; set; }
    public required string ScientificName { get; set; }
    public required string Name { get; set; }
    public string? NameInEnglish { get; set; }
    public Guid ClimateId { get; set; }
    public Guid PottingMixId { get; set; }
    public FloweringHabit FloweringHabit { get; set; }
    public required string FertilizationFrequency { get; set; }
    public string? FertilizationNotes { get; set; }
    public string? Comments { get; set; }

    /// <summary>Always present - the schema requires a care profile per species.</summary>
    public required SpeciesCareProfileDto Care { get; set; }

    /// <summary>Always present - the schema requires a toxicity profile per species.</summary>
    public required SpeciesToxicityProfileDto Toxicity { get; set; }

    /// <summary>Null when the species does not flower in cultivation.</summary>
    public SpeciesFloweringProfileDto? Flowering { get; set; }
}
