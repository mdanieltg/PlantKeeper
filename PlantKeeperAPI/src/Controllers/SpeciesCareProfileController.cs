using System.Net.Mime;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlantKeeperAPI.Authorization;
using PlantKeeperAPI.Database;
using PlantKeeperAPI.DataTransferObjects;
using PlantKeeperAPI.Entities;
using PlantKeeperAPI.Models;

namespace PlantKeeperAPI.Controllers;

/// <summary>
/// Singleton sub-resource: the profile shares its primary key with the species, so there
/// is no POST and no id of its own - PUT creates or replaces it.
/// <para>
/// No DELETE either. Every species is required to have a care profile, and removing one
/// would leave behind exactly the half-researched row the schema exists to prevent. To
/// drop the profile, delete the species.
/// </para>
/// </summary>
[ApiController]
[Route("/api/plant-species/{speciesId:guid}/care")]
[Consumes(MediaTypeNames.Application.Json)]
[Produces(MediaTypeNames.Application.Json)]
[RequiresPermission(Permissions.AlmanacRead)]
public class SpeciesCareProfileController : ControllerBase
{
    private readonly PlantKeeperDbContext _dbContext;
    private readonly IMapper _mapper;

    public SpeciesCareProfileController(PlantKeeperDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    [HttpGet]
    [ProducesResponseType<SpeciesCareProfileDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async ValueTask<ActionResult<SpeciesCareProfileDto>> Get([FromRoute] Guid speciesId)
    {
        SpeciesCareProfile? profile = await _dbContext.SpeciesCareProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(entry => entry.SpeciesId == speciesId);

        return profile is not null
            ? Ok(_mapper.Map<SpeciesCareProfileDto>(profile))
            : NotFound();
    }

    [RequiresPermission(Permissions.AlmanacPropose)]
    [HttpPut]
    [ProducesResponseType<SpeciesCareProfileDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<SpeciesCareProfileDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async ValueTask<ActionResult<SpeciesCareProfileDto>> Upsert([FromRoute] Guid speciesId,
        [FromBody] InputSpeciesCareProfile input)
    {
        if (!await _dbContext.PlantSpecies.AnyAsync(species => species.Id == speciesId)) return NotFound();

        SpeciesCareProfile? profile = await _dbContext.SpeciesCareProfiles
            .FirstOrDefaultAsync(entry => entry.SpeciesId == speciesId);

        bool created = profile is null;

        if (profile is null)
        {
            profile = _mapper.Map<SpeciesCareProfile>(input);
            profile.SpeciesId = speciesId;
            await _dbContext.SpeciesCareProfiles.AddAsync(profile);
        }
        else
        {
            _mapper.Map(input, profile);
        }

        await _dbContext.SaveChangesAsync();

        var profileToReturn = _mapper.Map<SpeciesCareProfileDto>(profile);

        return created
            ? CreatedAtAction(nameof(Get), new { speciesId }, profileToReturn)
            : Ok(profileToReturn);
    }
}
