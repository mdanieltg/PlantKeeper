using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using PlantKeeperAPI.Enums;

namespace PlantKeeperAPI.Models;

/// <summary>
/// Species are written as a whole aggregate. Care and toxicity are required in C# but the
/// foreign key sits on the dependent, so the database cannot insist a species has them -
/// accepting them here is what keeps an incomplete species from ever being created.
/// </summary>
[DebuggerDisplay("Species: {ScientificName}")]
public class InputPlantSpecies
{
    [StringLength(100)]
    public required string ScientificName { get; set; }

    /// <summary>Primary common name, in whichever language the collection uses for it.</summary>
    [StringLength(50)]
    public required string Name { get; set; }

    /// <summary>English equivalent where one exists and differs from <see cref="Name" />.</summary>
    [StringLength(50)]
    public string? NameInEnglish { get; set; }

    public Guid ClimateId { get; set; }
    public Guid PottingMixId { get; set; }

    public FloweringHabit FloweringHabit { get; set; }

    [StringLength(50)]
    public required string FertilizationFrequency { get; set; }

    [StringLength(255)]
    public string? FertilizationNotes { get; set; }

    [StringLength(255)]
    public string? Comments { get; set; }

    [Required]
    public required InputSpeciesCareProfile Care { get; set; }

    [Required]
    public required InputSpeciesToxicityProfile Toxicity { get; set; }

    /// <summary>Must be null when <see cref="FloweringHabit" /> is <c>DoesNotFlower</c>.</summary>
    public InputSpeciesFloweringProfile? Flowering { get; set; }
}
