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
[Route("/api/fertilizers")]
[Consumes(MediaTypeNames.Application.Json)]
[Produces(MediaTypeNames.Application.Json)]
[RequiresPermission(Permissions.AlmanacRead)]
public class FertilizersController : ControllerBase
{
    private readonly IAlmanacProposalService _almanac;
    private readonly PlantKeeperDbContext _dbContext;
    private readonly IMapper _mapper;

    public FertilizersController(PlantKeeperDbContext dbContext, IMapper mapper,
        IAlmanacProposalService almanac)
    {
        _dbContext = dbContext;
        _almanac = almanac;
        _mapper = mapper;
    }

    [HttpGet]
    [ProducesResponseType<IEnumerable<FertilizerDto>>(StatusCodes.Status200OK)]
    public IEnumerable<FertilizerDto> List() => _mapper.Map<IEnumerable<FertilizerDto>>(
        _dbContext.Fertilizers.OrderBy(fertilizer => fertilizer.Name)
    );

    [RequiresPermission(Permissions.AlmanacPropose)]
    [HttpPost]
    [ProducesResponseType<FertilizerDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType<AlmanacChangeProposalDto>(StatusCodes.Status202Accepted)]
    public async ValueTask<ActionResult<FertilizerDto>> Create([FromBody] InputFertilizer fertilizer)
    {
        if (await _almanac.SubmitAsync(AlmanacTargets.Fertilizer, AlmanacChangeOperation.Create,
                fertilizer) is { } queued) return Accepted(queued);

        var fertilizerToCreate = _mapper.Map<Fertilizer>(fertilizer);
        await _dbContext.Fertilizers.AddAsync(fertilizerToCreate);
        await _dbContext.SaveChangesAsync();

        var fertilizerToReturn = _mapper.Map<FertilizerDto>(fertilizerToCreate);
        return CreatedAtAction(nameof(Get), new { fertilizerId = fertilizerToReturn.Id }, fertilizerToReturn);
    }

    [HttpGet("{fertilizerId:guid}")]
    [ProducesResponseType<FertilizerDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async ValueTask<ActionResult<FertilizerDto>> Get([FromRoute] Guid fertilizerId)
    {
        Fertilizer? fertilizer = await _dbContext.Fertilizers.FindAsync(fertilizerId);
        return fertilizer is not null
            ? Ok(_mapper.Map<FertilizerDto>(fertilizer))
            : NotFound();
    }

    [RequiresPermission(Permissions.AlmanacPropose)]
    [HttpPut("{fertilizerId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType<AlmanacChangeProposalDto>(StatusCodes.Status202Accepted)]
    public async ValueTask<IActionResult> Update([FromRoute] Guid fertilizerId, [FromBody] InputFertilizer fertilizer)
    {
        Fertilizer? currentFertilizer = await _dbContext.Fertilizers.FindAsync(fertilizerId);
        if (currentFertilizer is null) return NotFound();

        if (await _almanac.SubmitAsync(AlmanacTargets.Fertilizer, AlmanacChangeOperation.Update,
                fertilizer, currentFertilizer.Id, currentFertilizer.Version) is { } queued) return Accepted(queued);

        _mapper.Map(fertilizer, currentFertilizer);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }

    [RequiresPermission(Permissions.AlmanacPropose)]
    [HttpDelete("{fertilizerId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<AlmanacChangeProposalDto>(StatusCodes.Status202Accepted)]
    public async ValueTask<IActionResult> Delete([FromRoute] Guid fertilizerId)
    {
        Fertilizer? fertilizer = await _dbContext.Fertilizers.FindAsync(fertilizerId);
        if (fertilizer is null) return NotFound();

        if (await _almanac.SubmitAsync(AlmanacTargets.Fertilizer, AlmanacChangeOperation.Delete,
                null, fertilizer.Id, fertilizer.Version) is { } queued) return Accepted(queued);

        return await this.DeleteAsync(_dbContext, fertilizer) ?? NoContent();
    }
}
