using System.Net.Mime;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlantKeeperAPI.Authorization;
using PlantKeeperAPI.DataTransferObjects;
using PlantKeeperAPI.Database;
using PlantKeeperAPI.Entities;
using PlantKeeperAPI.Enums;
using PlantKeeperAPI.Extensions;
using PlantKeeperAPI.Services;
using PlantKeeperAPI.Models;

namespace PlantKeeperAPI.Controllers;

/// <summary>
/// Singleton sub-resource, and the one profile that is genuinely optional: a species that
/// does not flower simply has no row. DELETE is therefore meaningful here, and a profile
/// on a <c>DoesNotFlower</c> species is rejected.
/// </summary>
[ApiController]
[Route("/api/plant-species/{speciesId:guid}/flowering")]
[Consumes(MediaTypeNames.Application.Json)]
[Produces(MediaTypeNames.Application.Json)]
[RequiresPermission(Permissions.AlmanacRead)]
public class SpeciesFloweringProfileController : ControllerBase
{
    private readonly IAlmanacProposalService _almanac;
    private readonly PlantKeeperDbContext _dbContext;
    private readonly IMapper _mapper;

    public SpeciesFloweringProfileController(PlantKeeperDbContext dbContext, IMapper mapper,
        IAlmanacProposalService almanac)
    {
        _dbContext = dbContext;
        _almanac = almanac;
        _mapper = mapper;
    }

    [HttpGet]
    [ProducesResponseType<SpeciesFloweringProfileDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async ValueTask<ActionResult<SpeciesFloweringProfileDto>> Get([FromRoute] Guid speciesId)
    {
        SpeciesFloweringProfile? profile = await _dbContext.SpeciesFloweringProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(entry => entry.SpeciesId == speciesId);

        return profile is not null
            ? Ok(_mapper.Map<SpeciesFloweringProfileDto>(profile))
            : NotFound();
    }

    [RequiresPermission(Permissions.AlmanacPropose)]
    [HttpPut]
    [ProducesResponseType<SpeciesFloweringProfileDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<SpeciesFloweringProfileDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType<AlmanacChangeProposalDto>(StatusCodes.Status202Accepted)]
    public async ValueTask<ActionResult<SpeciesFloweringProfileDto>> Upsert([FromRoute] Guid speciesId,
        [FromBody] InputSpeciesFloweringProfile input)
    {
        PlantSpecies? species = await _dbContext.PlantSpecies
            .AsNoTracking()
            .FirstOrDefaultAsync(entry => entry.Id == speciesId);

        if (species is null) return NotFound();

        if (species.FloweringHabit is FloweringHabit.DoesNotFlower)
        {
            ModelState.TryAddModelError("flowering",
                "A species whose flowering habit is DoesNotFlower cannot carry a flowering profile.");

            return UnprocessableEntity(new ValidationProblemDetails(ModelState));
        }

        SpeciesFloweringProfile? profile = await _dbContext.SpeciesFloweringProfiles
            .FirstOrDefaultAsync(entry => entry.SpeciesId == speciesId);

        if (await _almanac.SubmitAsync(AlmanacTargets.SpeciesFloweringProfile, AlmanacChangeOperation.Update,
                input, speciesId, profile?.Version) is { } queued) return Accepted(queued);

        bool created = profile is null;

        if (profile is null)
        {
            profile = _mapper.Map<SpeciesFloweringProfile>(input);
            profile.SpeciesId = speciesId;
            await _dbContext.SpeciesFloweringProfiles.AddAsync(profile);
        }
        else
        {
            _mapper.Map(input, profile);
        }

        await _dbContext.SaveChangesAsync();

        var profileToReturn = _mapper.Map<SpeciesFloweringProfileDto>(profile);

        return created
            ? CreatedAtAction(nameof(Get), new { speciesId }, profileToReturn)
            : Ok(profileToReturn);
    }

    [RequiresPermission(Permissions.AlmanacPropose)]
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<AlmanacChangeProposalDto>(StatusCodes.Status202Accepted)]
    public async ValueTask<IActionResult> Delete([FromRoute] Guid speciesId)
    {
        SpeciesFloweringProfile? profile = await _dbContext.SpeciesFloweringProfiles
            .FirstOrDefaultAsync(entry => entry.SpeciesId == speciesId);

        if (profile is null) return NotFound();

        if (await _almanac.SubmitAsync(AlmanacTargets.SpeciesFloweringProfile, AlmanacChangeOperation.Delete,
                null, speciesId, profile.Version) is { } queued) return Accepted(queued);

        return await this.DeleteAsync(_dbContext, profile) ?? NoContent();
    }
}
