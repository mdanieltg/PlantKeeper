using Mapster;
using PlantKeeperAPI.DataTransferObjects;
using PlantKeeperAPI.Entities;
using PlantKeeperAPI.Extensions;
using PlantKeeperAPI.Models;

namespace PlantKeeperAPI.Mapping;

/// <summary>Plant instances and the logs hanging off them.</summary>
public class PlantMappings : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Plant, PlantDto>();
        config.NewConfig<InputPlant, Plant>()
            .IgnoreOwnership()
            .IgnoreNavigations()
            .Ignore(plant => plant.Id);

        // WateringLog is the one genuine name mismatch: the entity calls it MethodId.
        // Left to convention, Mapster would flatten WateringMethodId from the unloaded
        // WateringMethod navigation and quietly hand back Guid.Empty.
        config.NewConfig<WateringLog, WateringLogDto>()
            .Map(dto => dto.WateringMethodId, log => log.MethodId);
        config.NewConfig<InputWateringLog, WateringLog>()
            .IgnoreOwnership()
            .Map(log => log.MethodId, input => input.WateringMethodId)
            .IgnoreNavigations()
            .Ignore(log => log.Id);

        config.NewConfig<FertilizationLog, FertilizationLogDto>();
        config.NewConfig<InputFertilizationLog, FertilizationLog>()
            .IgnoreOwnership()
            .IgnoreNavigations()
            .Ignore(log => log.Id);

        config.NewConfig<TreatmentLog, TreatmentLogDto>();
        config.NewConfig<InputTreatmentLog, TreatmentLog>()
            .IgnoreOwnership()
            .IgnoreNavigations()
            .Ignore(log => log.Id);

        config.NewConfig<RepottingLog, RepottingLogDto>();
        config.NewConfig<InputRepottingLog, RepottingLog>()
            .IgnoreOwnership()
            .IgnoreNavigations()
            .Ignore(log => log.Id);

        config.NewConfig<ObservationLog, ObservationLogDto>();
        config.NewConfig<InputObservationLog, ObservationLog>()
            .IgnoreOwnership()
            .IgnoreNavigations()
            .Ignore(log => log.Id);

        config.NewConfig<GrowthLog, GrowthLogDto>();
        config.NewConfig<InputGrowthLog, GrowthLog>()
            .IgnoreOwnership()
            .IgnoreNavigations()
            .Ignore(log => log.Id);
    }
}
