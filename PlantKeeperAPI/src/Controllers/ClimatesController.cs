using System.Net.Mime;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using PlantKeeperAPI.Authorization;
using PlantKeeperAPI.DataTransferObjects;
using PlantKeeperAPI.Database;
using PlantKeeperAPI.Entities;
using PlantKeeperAPI.Enums;
using PlantKeeperAPI.Extensions;
using PlantKeeperAPI.Models;
using PlantKeeperAPI.Services;

namespace PlantKeeperAPI.Controllers;

[ApiController]
[Route("/api/climates")]
[Consumes(MediaTypeNames.Application.Json)]
[Produces(MediaTypeNames.Application.Json)]
[RequiresPermission(Permissions.AlmanacRead)]
public class ClimatesController : ControllerBase
{
    private readonly IAlmanacProposalService _almanac;
    private readonly PlantKeeperDbContext _dbContext;
    private readonly IMapper _mapper;

    public ClimatesController(PlantKeeperDbContext dbContext, IMapper mapper,
        IAlmanacProposalService almanac)
    {
        _dbContext = dbContext;
        _almanac = almanac;
        _mapper = mapper;
    }

    [HttpGet]
    [ProducesResponseType<IEnumerable<ClimateDto>>(StatusCodes.Status200OK)]
    public IEnumerable<ClimateDto> List() => _mapper.Map<IEnumerable<ClimateDto>>(
        _dbContext.Climates.OrderBy(climate => climate.Name)
    );

    [RequiresPermission(Permissions.AlmanacPropose)]
    [HttpPost]
    [ProducesResponseType<ClimateDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType<AlmanacChangeProposalDto>(StatusCodes.Status202Accepted)]
    public async ValueTask<ActionResult<ClimateDto>> Create([FromBody] InputClimate climate)
    {
        if (await _almanac.SubmitAsync(AlmanacTargets.Climate, AlmanacChangeOperation.Create,
                climate) is { } queued) return Accepted(queued);

        var climateToCreate = _mapper.Map<Climate>(climate);
        await _dbContext.Climates.AddAsync(climateToCreate);
        await _dbContext.SaveChangesAsync();

        var climateToReturn = _mapper.Map<ClimateDto>(climateToCreate);
        return CreatedAtAction(nameof(Get), new { climateId = climateToReturn.Id }, climateToReturn);
    }

    [HttpGet("{climateId:guid}")]
    [ProducesResponseType<ClimateDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async ValueTask<ActionResult<ClimateDto>> Get([FromRoute] Guid climateId)
    {
        Climate? climate = await _dbContext.Climates.FindAsync(climateId);
        return climate is not null
            ? Ok(_mapper.Map<ClimateDto>(climate))
            : NotFound();
    }

    [RequiresPermission(Permissions.AlmanacPropose)]
    [HttpPut("{climateId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType<AlmanacChangeProposalDto>(StatusCodes.Status202Accepted)]
    public async ValueTask<IActionResult> Update([FromRoute] Guid climateId, [FromBody] InputClimate climate)
    {
        Climate? currentClimate = await _dbContext.Climates.FindAsync(climateId);
        if (currentClimate is null) return NotFound();

        if (await _almanac.SubmitAsync(AlmanacTargets.Climate, AlmanacChangeOperation.Update,
                climate, currentClimate.Id, currentClimate.Version) is { } queued) return Accepted(queued);

        _mapper.Map(climate, currentClimate);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }

    [RequiresPermission(Permissions.AlmanacPropose)]
    [HttpDelete("{climateId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<AlmanacChangeProposalDto>(StatusCodes.Status202Accepted)]
    public async ValueTask<IActionResult> Delete([FromRoute] Guid climateId)
    {
        Climate? climate = await _dbContext.Climates.FindAsync(climateId);
        if (climate is null) return NotFound();

        if (await _almanac.SubmitAsync(AlmanacTargets.Climate, AlmanacChangeOperation.Delete,
                null, climate.Id, climate.Version) is { } queued) return Accepted(queued);

        return await this.DeleteAsync(_dbContext, climate) ?? NoContent();
    }
}
