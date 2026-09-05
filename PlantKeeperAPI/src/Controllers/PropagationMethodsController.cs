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
[Route("/api/propagation-methods")]
[Consumes(MediaTypeNames.Application.Json)]
[Produces(MediaTypeNames.Application.Json)]
[RequiresPermission(Permissions.AlmanacRead)]
public class PropagationMethodsController : ControllerBase
{
    private readonly IAlmanacProposalService _almanac;
    private readonly PlantKeeperDbContext _dbContext;
    private readonly IMapper _mapper;

    public PropagationMethodsController(PlantKeeperDbContext dbContext, IMapper mapper,
        IAlmanacProposalService almanac)
    {
        _dbContext = dbContext;
        _almanac = almanac;
        _mapper = mapper;
    }

    [HttpGet]
    [ProducesResponseType<IEnumerable<PropagationMethodDto>>(StatusCodes.Status200OK)]
    public IEnumerable<PropagationMethodDto> List() => _mapper.Map<IEnumerable<PropagationMethodDto>>(
        _dbContext.PropagationMethods.OrderBy(method => method.Name)
    );

    [RequiresPermission(Permissions.AlmanacPropose)]
    [HttpPost]
    [ProducesResponseType<PropagationMethodDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType<AlmanacChangeProposalDto>(StatusCodes.Status202Accepted)]
    public async ValueTask<ActionResult<PropagationMethodDto>> Create([FromBody] InputPropagationMethod method)
    {
        if (await _almanac.SubmitAsync(AlmanacTargets.PropagationMethod, AlmanacChangeOperation.Create,
                method) is { } queued) return Accepted(queued);

        var methodToCreate = _mapper.Map<PropagationMethod>(method);
        await _dbContext.PropagationMethods.AddAsync(methodToCreate);
        await _dbContext.SaveChangesAsync();

        var methodToReturn = _mapper.Map<PropagationMethodDto>(methodToCreate);
        return CreatedAtAction(nameof(Get), new { methodId = methodToReturn.Id }, methodToReturn);
    }

    [HttpGet("{methodId:guid}")]
    [ProducesResponseType<PropagationMethodDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async ValueTask<ActionResult<PropagationMethodDto>> Get([FromRoute] Guid methodId)
    {
        PropagationMethod? method = await _dbContext.PropagationMethods.FindAsync(methodId);
        return method is not null
            ? Ok(_mapper.Map<PropagationMethodDto>(method))
            : NotFound();
    }

    [RequiresPermission(Permissions.AlmanacPropose)]
    [HttpPut("{methodId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType<AlmanacChangeProposalDto>(StatusCodes.Status202Accepted)]
    public async ValueTask<IActionResult> Update([FromRoute] Guid methodId, [FromBody] InputPropagationMethod method)
    {
        PropagationMethod? currentPropagationMethod = await _dbContext.PropagationMethods.FindAsync(methodId);
        if (currentPropagationMethod is null) return NotFound();

        if (await _almanac.SubmitAsync(AlmanacTargets.PropagationMethod, AlmanacChangeOperation.Update,
                method, currentPropagationMethod.Id, currentPropagationMethod.Version) is { } queued) return Accepted(queued);

        _mapper.Map(method, currentPropagationMethod);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }

    [RequiresPermission(Permissions.AlmanacPropose)]
    [HttpDelete("{methodId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<AlmanacChangeProposalDto>(StatusCodes.Status202Accepted)]
    public async ValueTask<IActionResult> Delete([FromRoute] Guid methodId)
    {
        PropagationMethod? method = await _dbContext.PropagationMethods.FindAsync(methodId);
        if (method is null) return NotFound();

        if (await _almanac.SubmitAsync(AlmanacTargets.PropagationMethod, AlmanacChangeOperation.Delete,
                null, method.Id, method.Version) is { } queued) return Accepted(queued);

        return await this.DeleteAsync(_dbContext, method) ?? NoContent();
    }
}
