using Mapster;
using PlantKeeperAPI.DataTransferObjects;
using PlantKeeperAPI.Entities;
using PlantKeeperAPI.Extensions;
using PlantKeeperAPI.Models;

namespace PlantKeeperAPI.Mapping;

/// <summary>Lookup tables and the ecosystem catalogue.</summary>
public class ReferenceDataMappings : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Climate, ClimateDto>();
        config.NewConfig<InputClimate, Climate>()
            .IgnoreNavigations()
            .Ignore(climate => climate.Id);

        config.NewConfig<PottingMix, PottingMixDto>();
        config.NewConfig<InputPottingMix, PottingMix>()
            .IgnoreNavigations()
            .Ignore(mix => mix.Id);

        config.NewConfig<WateringMethod, WateringMethodDto>();
        config.NewConfig<InputWateringMethod, WateringMethod>()
            .IgnoreNavigations()
            .Ignore(method => method.Id);

        config.NewConfig<Fertilizer, FertilizerDto>();
        config.NewConfig<InputFertilizer, Fertilizer>()
            .IgnoreNavigations()
            .Ignore(fertilizer => fertilizer.Id);

        config.NewConfig<Treatment, TreatmentDto>();
        config.NewConfig<InputTreatment, Treatment>()
            .IgnoreNavigations()
            .Ignore(treatment => treatment.Id);

        config.NewConfig<PropagationMethod, PropagationMethodDto>();
        config.NewConfig<InputPropagationMethod, PropagationMethod>()
            .IgnoreNavigations()
            .Ignore(method => method.Id);

        config.NewConfig<Pest, PestDto>();
        config.NewConfig<InputPest, Pest>()
            .IgnoreNavigations()
            .Ignore(pest => pest.Id);

        config.NewConfig<BeneficialOrganism, BeneficialOrganismDto>();
        config.NewConfig<InputBeneficialOrganism, BeneficialOrganism>()
            .IgnoreNavigations()
            .Ignore(organism => organism.Id);
    }
}
