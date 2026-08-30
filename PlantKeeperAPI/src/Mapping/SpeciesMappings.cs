using Mapster;
using PlantKeeperAPI.DataTransferObjects;
using PlantKeeperAPI.Entities;
using PlantKeeperAPI.Extensions;
using PlantKeeperAPI.Models;

namespace PlantKeeperAPI.Mapping;

/// <summary>The species aggregate, its profiles, and the three recommendation matrices.</summary>
public class SpeciesMappings : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // Care, Toxicity and Flowering are entity-typed, so IgnoreNavigations drops them
        // from the write side on purpose - PlantSpeciesService owns the profile rows.
        config.NewConfig<PlantSpecies, PlantSpeciesDto>();
        config.NewConfig<InputPlantSpecies, PlantSpecies>()
            .IgnoreNavigations()
            .Ignore(species => species.Id);

        config.NewConfig<SpeciesCareProfile, SpeciesCareProfileDto>();
        config.NewConfig<InputSpeciesCareProfile, SpeciesCareProfile>()
            .IgnoreNavigations()
            .Ignore(profile => profile.SpeciesId);

        config.NewConfig<SpeciesToxicityProfile, SpeciesToxicityProfileDto>();
        config.NewConfig<InputSpeciesToxicityProfile, SpeciesToxicityProfile>()
            .IgnoreNavigations()
            .Ignore(profile => profile.SpeciesId);

        config.NewConfig<SpeciesFloweringProfile, SpeciesFloweringProfileDto>();
        config.NewConfig<InputSpeciesFloweringProfile, SpeciesFloweringProfile>()
            .IgnoreNavigations()
            .Ignore(profile => profile.SpeciesId);

        // The matrices take their species from the route, never from the body.
        config.NewConfig<SpeciesFertilizerRecommendation, SpeciesFertilizerRecommendationDto>();
        config.NewConfig<InputSpeciesFertilizerRecommendation, SpeciesFertilizerRecommendation>()
            .IgnoreNavigations()
            .Ignore(recommendation => recommendation.Id, recommendation => recommendation.SpeciesId);

        config.NewConfig<SpeciesTreatmentRecommendation, SpeciesTreatmentRecommendationDto>();
        config.NewConfig<InputSpeciesTreatmentRecommendation, SpeciesTreatmentRecommendation>()
            .IgnoreNavigations()
            .Ignore(recommendation => recommendation.Id, recommendation => recommendation.SpeciesId);

        config.NewConfig<SpeciesPropagationMethod, SpeciesPropagationMethodDto>();
        config.NewConfig<InputSpeciesPropagationMethod, SpeciesPropagationMethod>()
            .IgnoreNavigations()
            .Ignore(link => link.Id, link => link.SpeciesId);

        config.NewConfig<PropagationBatch, PropagationBatchDto>();
        config.NewConfig<InputPropagationBatch, PropagationBatch>()
            .IgnoreNavigations()
            .Ignore(batch => batch.Id);
    }
}
