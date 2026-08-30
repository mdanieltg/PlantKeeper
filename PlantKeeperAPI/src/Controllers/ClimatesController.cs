using System.Net.Mime;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using PlantKeeperAPI.Database;
using PlantKeeperAPI.DataTransferObjects;
using PlantKeeperAPI.Entities;
using PlantKeeperAPI.Models;

namespace PlantKeeperAPI.Controllers;

[ApiController]
[Route("/api/climates")]
[Consumes(MediaTypeNames.Application.Json)]
[Produces(MediaTypeNames.Application.Json)]
public class ClimatesController : ControllerBase
{
    private readonly PlantKeeperDbContext _dbContext;
    private readonly IMapper _mapper;

    public ClimatesController(PlantKeeperDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    [HttpGet]
    [ProducesResponseType<IEnumerable<ClimateDto>>(StatusCodes.Status200OK)]
    public IEnumerable<ClimateDto> List() => _mapper.Map<IEnumerable<ClimateDto>>(
        _dbContext.Climates.OrderBy(climate => climate.Name)
    );

    [HttpPost]
    [ProducesResponseType<ClimateDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async ValueTask<ActionResult<ClimateDto>> Create([FromBody] InputClimate climate)
    {
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

    [HttpPut("{climateId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async ValueTask<IActionResult> Update([FromRoute] Guid climateId, [FromBody] InputClimate climate)
    {
        Climate? currentClimate = await _dbContext.Climates.FindAsync(climateId);
        if (currentClimate is null) return NotFound();

        _mapper.Map(climate, currentClimate);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{climateId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async ValueTask<IActionResult> Delete([FromRoute] Guid climateId)
    {
        Climate? climate = await _dbContext.Climates.FindAsync(climateId);
        if (climate is null) return NotFound();

        _dbContext.Remove(climate);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }
}
