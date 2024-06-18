using AutoMapper;
using PlantKeeperAPI.DataTransferObjects;
using PlantKeeperAPI.Entities;

namespace PlantKeeperAPI.MappingProfiles;

public class Profiles : Profile
{
    public Profiles()
    {
        // Keeper
        CreateMap<Keeper, KeeperDto>();

        // Plant
        CreateMap<Plant, PlantDto>();

        // WateringMethod
        CreateMap<WateringMethod, WateringMethodDto>();

        // WateringLog
        CreateMap<WateringLog, WateringLogDto>();
    }
}
