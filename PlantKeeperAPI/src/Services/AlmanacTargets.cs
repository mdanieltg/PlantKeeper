using PlantKeeperAPI.Entities;

namespace PlantKeeperAPI.Services;

/// <summary>
/// The names a proposal's <see cref="AlmanacChangeProposal.TargetType" /> may take.
/// <para>
/// Entity type names for the rows themselves, and a dotted name for each payload-free set
/// that is replaced wholesale. Constants rather than <c>nameof</c> at each call site, so a
/// renamed entity does not silently orphan the proposals already recorded against it.
/// </para>
/// </summary>
public static class AlmanacTargets
{
    public const string PlantSpecies = nameof(Entities.PlantSpecies);
    public const string Climate = nameof(Entities.Climate);
    public const string PottingMix = nameof(Entities.PottingMix);
    public const string WateringMethod = nameof(Entities.WateringMethod);
    public const string Fertilizer = nameof(Entities.Fertilizer);
    public const string Treatment = nameof(Entities.Treatment);
    public const string PropagationMethod = nameof(Entities.PropagationMethod);
    public const string Pest = nameof(Entities.Pest);
    public const string BeneficialOrganism = nameof(Entities.BeneficialOrganism);

    public const string SpeciesCareProfile = nameof(Entities.SpeciesCareProfile);
    public const string SpeciesToxicityProfile = nameof(Entities.SpeciesToxicityProfile);
    public const string SpeciesFloweringProfile = nameof(Entities.SpeciesFloweringProfile);

    public const string SpeciesFertilizerRecommendation = nameof(Entities.SpeciesFertilizerRecommendation);
    public const string SpeciesTreatmentRecommendation = nameof(Entities.SpeciesTreatmentRecommendation);
    public const string SpeciesPropagationMethod = nameof(Entities.SpeciesPropagationMethod);

    /// <summary>Replacing the whole set of treatments effective against a pest.</summary>
    public const string PestTreatments = "Pest.Treatments";

    /// <summary>Replacing the whole set of species a beneficial organism supports.</summary>
    public const string OrganismSpecies = "BeneficialOrganism.Species";

    /// <summary>Replacing the whole set of pests a beneficial organism controls.</summary>
    public const string OrganismPests = "BeneficialOrganism.Pests";
}
