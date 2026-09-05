using System.Net.Mime;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlantKeeperAPI.Authorization;
using PlantKeeperAPI.DataTransferObjects;
using PlantKeeperAPI.Database;
using PlantKeeperAPI.Entities;
using PlantKeeperAPI.Enums;
using PlantKeeperAPI.Services;
using PlantKeeperAPI.Models;

namespace PlantKeeperAPI.Controllers;

/// <summary>
/// Singleton sub-resource: the profile shares its primary key with the species, so there
/// is no POST and no id of its own - PUT creates or replaces it.
/// <para>
/// No DELETE either. Every species is required to have a toxicity profile, and removing one
/// would leave behind exactly the unresearched species that would read as harmless. To
/// drop the profile, delete the species.
/// </para>
/// </summary>
[ApiController]
[Route("/api/plant-species/{speciesId:guid}/toxicity")]
[Consumes(MediaTypeNames.Application.Json)]
[Produces(MediaTypeNames.Application.Json)]
[RequiresPermission(Permissions.AlmanacRead)]
public class SpeciesToxicityProfileController : ControllerBase
{
    private readonly IAlmanacProposalService _almanac;
    private readonly PlantKeeperDbContext _dbContext;
    private readonly IMapper _mapper;

    public SpeciesToxicityProfileController(PlantKeeperDbContext dbContext, IMapper mapper,
        IAlmanacProposalService almanac)
    {
        _dbContext = dbContext;
        _almanac = almanac;
        _mapper = mapper;
    }

    [HttpGet]
    [ProducesResponseType<SpeciesToxicityProfileDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async ValueTask<ActionResult<SpeciesToxicityProfileDto>> Get([FromRoute] Guid speciesId)
    {
        SpeciesToxicityProfile? profile = await _dbContext.SpeciesToxicityProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(entry => entry.SpeciesId == speciesId);

        return profile is not null
            ? Ok(_mapper.Map<SpeciesToxicityProfileDto>(profile))
            : NotFound();
    }

    [RequiresPermission(Permissions.AlmanacPropose)]
    [HttpPut]
    [ProducesResponseType<SpeciesToxicityProfileDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<SpeciesToxicityProfileDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType<AlmanacChangeProposalDto>(StatusCodes.Status202Accepted)]
    public async ValueTask<ActionResult<SpeciesToxicityProfileDto>> Upsert([FromRoute] Guid speciesId,
        [FromBody] InputSpeciesToxicityProfile input)
    {
        if (!await _dbContext.PlantSpecies.AnyAsync(species => species.Id == speciesId)) return NotFound();

        SpeciesToxicityProfile? profile = await _dbContext.SpeciesToxicityProfiles
            .FirstOrDefaultAsync(entry => entry.SpeciesId == speciesId);

        if (await _almanac.SubmitAsync(AlmanacTargets.SpeciesToxicityProfile, AlmanacChangeOperation.Update,
                input, speciesId, profile?.Version) is { } queued) return Accepted(queued);

        bool created = profile is null;

        if (profile is null)
        {
            profile = _mapper.Map<SpeciesToxicityProfile>(input);
            profile.SpeciesId = speciesId;
            await _dbContext.SpeciesToxicityProfiles.AddAsync(profile);
        }
        else
        {
            _mapper.Map(input, profile);
        }

        await _dbContext.SaveChangesAsync();

        var profileToReturn = _mapper.Map<SpeciesToxicityProfileDto>(profile);

        return created
            ? CreatedAtAction(nameof(Get), new { speciesId }, profileToReturn)
            : Ok(profileToReturn);
    }
}
