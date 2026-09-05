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
[Route("/api/watering-methods")]
[Consumes(MediaTypeNames.Application.Json)]
[Produces(MediaTypeNames.Application.Json)]
[RequiresPermission(Permissions.AlmanacRead)]
public class WateringMethodsController : ControllerBase
{
    private readonly IAlmanacProposalService _almanac;
    private readonly PlantKeeperDbContext _dbContext;
    private readonly IMapper _mapper;

    public WateringMethodsController(PlantKeeperDbContext dbContext, IMapper mapper,
        IAlmanacProposalService almanac)
    {
        _dbContext = dbContext;
        _almanac = almanac;
        _mapper = mapper;
    }

    [HttpGet]
    [ProducesResponseType<IEnumerable<WateringMethodDto>>(StatusCodes.Status200OK)]
    public IEnumerable<WateringMethodDto> List() => _mapper.Map<IEnumerable<WateringMethodDto>>(
        _dbContext.WateringMethods.OrderBy(method => method.Name)
    );

    [RequiresPermission(Permissions.AlmanacPropose)]
    [HttpPost]
    [ProducesResponseType<WateringMethodDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType<AlmanacChangeProposalDto>(StatusCodes.Status202Accepted)]
    public async ValueTask<ActionResult<WateringMethodDto>> Create([FromBody] InputWateringMethod method)
    {
        if (await _almanac.SubmitAsync(AlmanacTargets.WateringMethod, AlmanacChangeOperation.Create,
                method) is { } queued) return Accepted(queued);

        var methodToCreate = _mapper.Map<WateringMethod>(method);
        await _dbContext.WateringMethods.AddAsync(methodToCreate);
        await _dbContext.SaveChangesAsync();

        var methodToReturn = _mapper.Map<WateringMethodDto>(methodToCreate);
        return CreatedAtAction(nameof(Get), new { wateringMethodId = methodToReturn.Id }, methodToReturn);
    }

    [HttpGet("{wateringMethodId:guid}")]
    [ProducesResponseType<WateringMethodDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async ValueTask<ActionResult<WateringMethodDto>> Get([FromRoute] Guid wateringMethodId)
    {
        WateringMethod? method = await _dbContext.WateringMethods.FindAsync(wateringMethodId);
        return method is not null
            ? Ok(_mapper.Map<WateringMethodDto>(method))
            : NotFound();
    }

    [RequiresPermission(Permissions.AlmanacPropose)]
    [HttpPut("{wateringMethodId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType<AlmanacChangeProposalDto>(StatusCodes.Status202Accepted)]
    public async ValueTask<IActionResult> Update([FromRoute] Guid wateringMethodId,
        [FromBody] InputWateringMethod method)
    {
        WateringMethod? currentWateringMethod = await _dbContext.WateringMethods.FindAsync(wateringMethodId);
        if (currentWateringMethod is null) return NotFound();

        if (await _almanac.SubmitAsync(AlmanacTargets.WateringMethod, AlmanacChangeOperation.Update,
                method, currentWateringMethod.Id, currentWateringMethod.Version) is { } queued) return Accepted(queued);

        _mapper.Map(method, currentWateringMethod);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }

    [RequiresPermission(Permissions.AlmanacPropose)]
    [HttpDelete("{wateringMethodId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<AlmanacChangeProposalDto>(StatusCodes.Status202Accepted)]
    public async ValueTask<IActionResult> Delete([FromRoute] Guid wateringMethodId)
    {
        WateringMethod? method = await _dbContext.WateringMethods.FindAsync(wateringMethodId);
        if (method is null) return NotFound();

        if (await _almanac.SubmitAsync(AlmanacTargets.WateringMethod, AlmanacChangeOperation.Delete,
                null, method.Id, method.Version) is { } queued) return Accepted(queued);

        return await this.DeleteAsync(_dbContext, method) ?? NoContent();
    }
}
