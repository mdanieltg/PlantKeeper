using System.Net.Mime;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using PlantKeeperAPI.Database;
using PlantKeeperAPI.DataTransferObjects;
using PlantKeeperAPI.Entities;
using PlantKeeperAPI.Models;

namespace PlantKeeperAPI.Controllers;

[ApiController]
[Route("/api/potting-mixes")]
[Consumes(MediaTypeNames.Application.Json)]
[Produces(MediaTypeNames.Application.Json)]
public class PottingMixesController : ControllerBase
{
    private readonly PlantKeeperDbContext _dbContext;
    private readonly IMapper _mapper;

    public PottingMixesController(PlantKeeperDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    [HttpGet]
    [ProducesResponseType<IEnumerable<PottingMixDto>>(StatusCodes.Status200OK)]
    public IEnumerable<PottingMixDto> List() => _mapper.Map<IEnumerable<PottingMixDto>>(
        _dbContext.PottingMixes.OrderBy(pottingMix => pottingMix.Name)
    );

    [HttpPost]
    [ProducesResponseType<PottingMixDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async ValueTask<ActionResult<PottingMixDto>> Create([FromBody] InputPottingMix pottingMix)
    {
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

    [HttpPut("{pottingMixId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async ValueTask<IActionResult> Update([FromRoute] Guid pottingMixId, [FromBody] InputPottingMix pottingMix)
    {
        PottingMix? currentPottingMix = await _dbContext.PottingMixes.FindAsync(pottingMixId);
        if (currentPottingMix is null) return NotFound();

        _mapper.Map(pottingMix, currentPottingMix);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{pottingMixId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async ValueTask<IActionResult> Delete([FromRoute] Guid pottingMixId)
    {
        PottingMix? pottingMix = await _dbContext.PottingMixes.FindAsync(pottingMixId);
        if (pottingMix is null) return NotFound();

        _dbContext.Remove(pottingMix);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }
}
