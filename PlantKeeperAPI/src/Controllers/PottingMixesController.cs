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
[Route("/api/potting-mixes")]
[Consumes(MediaTypeNames.Application.Json)]
[Produces(MediaTypeNames.Application.Json)]
[RequiresPermission(Permissions.AlmanacRead)]
public class PottingMixesController : ControllerBase
{
    private readonly IAlmanacProposalService _almanac;
    private readonly PlantKeeperDbContext _dbContext;
    private readonly IMapper _mapper;

    public PottingMixesController(PlantKeeperDbContext dbContext, IMapper mapper,
        IAlmanacProposalService almanac)
    {
        _dbContext = dbContext;
        _almanac = almanac;
        _mapper = mapper;
    }

    [HttpGet]
    [ProducesResponseType<IEnumerable<PottingMixDto>>(StatusCodes.Status200OK)]
    public IEnumerable<PottingMixDto> List() => _mapper.Map<IEnumerable<PottingMixDto>>(
        _dbContext.PottingMixes.OrderBy(pottingMix => pottingMix.Name)
    );

    [RequiresPermission(Permissions.AlmanacPropose)]
    [HttpPost]
    [ProducesResponseType<PottingMixDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType<AlmanacChangeProposalDto>(StatusCodes.Status202Accepted)]
    public async ValueTask<ActionResult<PottingMixDto>> Create([FromBody] InputPottingMix pottingMix)
    {
        if (await _almanac.SubmitAsync(AlmanacTargets.PottingMix, AlmanacChangeOperation.Create,
                pottingMix) is { } queued) return Accepted(queued);

        var pottingMixToCreate = _mapper.Map<PottingMix>(pottingMix);
        await _dbContext.PottingMixes.AddAsync(pottingMixToCreate);
        await _dbContext.SaveChangesAsync();

        var pottingMixToReturn = _mapper.Map<PottingMixDto>(pottingMixToCreate);
        return CreatedAtAction(nameof(Get), new { pottingMixId = pottingMixToReturn.Id }, pottingMixToReturn);
    }

    [HttpGet("{pottingMixId:guid}")]
    [ProducesResponseType<PottingMixDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async ValueTask<ActionResult<PottingMixDto>> Get([FromRoute] Guid pottingMixId)
    {
        PottingMix? pottingMix = await _dbContext.PottingMixes.FindAsync(pottingMixId);
        return pottingMix is not null
            ? Ok(_mapper.Map<PottingMixDto>(pottingMix))
            : NotFound();
    }

    [RequiresPermission(Permissions.AlmanacPropose)]
    [HttpPut("{pottingMixId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType<AlmanacChangeProposalDto>(StatusCodes.Status202Accepted)]
    public async ValueTask<IActionResult> Update([FromRoute] Guid pottingMixId, [FromBody] InputPottingMix pottingMix)
    {
        PottingMix? currentPottingMix = await _dbContext.PottingMixes.FindAsync(pottingMixId);
        if (currentPottingMix is null) return NotFound();

        if (await _almanac.SubmitAsync(AlmanacTargets.PottingMix, AlmanacChangeOperation.Update,
                pottingMix, currentPottingMix.Id, currentPottingMix.Version) is { } queued) return Accepted(queued);

        _mapper.Map(pottingMix, currentPottingMix);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }

    [RequiresPermission(Permissions.AlmanacPropose)]
    [HttpDelete("{pottingMixId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<AlmanacChangeProposalDto>(StatusCodes.Status202Accepted)]
    public async ValueTask<IActionResult> Delete([FromRoute] Guid pottingMixId)
    {
        PottingMix? pottingMix = await _dbContext.PottingMixes.FindAsync(pottingMixId);
        if (pottingMix is null) return NotFound();

        if (await _almanac.SubmitAsync(AlmanacTargets.PottingMix, AlmanacChangeOperation.Delete,
                null, pottingMix.Id, pottingMix.Version) is { } queued) return Accepted(queued);

        return await this.DeleteAsync(_dbContext, pottingMix) ?? NoContent();
    }
}
