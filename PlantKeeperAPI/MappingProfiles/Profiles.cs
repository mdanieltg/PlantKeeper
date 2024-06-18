﻿using AutoMapper;
using PlantKeeperAPI.DataTransferObjects;
using PlantKeeperAPI.Entities;
using PlantKeeperAPI.Models;

namespace PlantKeeperAPI.MappingProfiles;

public class Profiles : Profile
{
    public Profiles()
    {
        // Keeper
        CreateMap<Keeper, KeeperDto>();

        // Plant
        CreateMap<Plant, PlantDto>();
        CreateMap<InputPlant, Plant>();

        // WateringMethod
        CreateMap<WateringMethod, WateringMethodDto>();

        // WateringLog
        CreateMap<WateringLog, WateringLogDto>();
    }
}
