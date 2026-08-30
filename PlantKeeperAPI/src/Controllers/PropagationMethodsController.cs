using System.Net.Mime;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using PlantKeeperAPI.Database;
using PlantKeeperAPI.DataTransferObjects;
using PlantKeeperAPI.Entities;
using PlantKeeperAPI.Models;

namespace PlantKeeperAPI.Controllers;

[ApiController]
[Route("/api/propagation-methods")]
[Consumes(MediaTypeNames.Application.Json)]
[Produces(MediaTypeNames.Application.Json)]
public class PropagationMethodsController : ControllerBase
{
    private readonly PlantKeeperDbContext _dbContext;
    private readonly IMapper _mapper;

    public PropagationMethodsController(PlantKeeperDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    [HttpGet]
    [ProducesResponseType<IEnumerable<PropagationMethodDto>>(StatusCodes.Status200OK)]
    public IEnumerable<PropagationMethodDto> List() => _mapper.Map<IEnumerable<PropagationMethodDto>>(
        _dbContext.PropagationMethods.OrderBy(method => method.Name)
    );

    [HttpPost]
    [ProducesResponseType<PropagationMethodDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async ValueTask<ActionResult<PropagationMethodDto>> Create([FromBody] InputPropagationMethod method)
    {
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

    [HttpPut("{methodId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async ValueTask<IActionResult> Update([FromRoute] Guid methodId, [FromBody] InputPropagationMethod method)
    {
        PropagationMethod? currentPropagationMethod = await _dbContext.PropagationMethods.FindAsync(methodId);
        if (currentPropagationMethod is null) return NotFound();

        _mapper.Map(method, currentPropagationMethod);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{methodId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async ValueTask<IActionResult> Delete([FromRoute] Guid methodId)
    {
        PropagationMethod? method = await _dbContext.PropagationMethods.FindAsync(methodId);
        if (method is null) return NotFound();

        _dbContext.Remove(method);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }
}
