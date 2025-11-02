using System.Net.Mime;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using PlantKeeperAPI.Database;
using PlantKeeperAPI.DataTransferObjects;
using PlantKeeperAPI.Entities;
using PlantKeeperAPI.Models;

namespace PlantKeeperAPI.Controllers;

[ApiController]
[Route("/api/plants")]
[Produces(MediaTypeNames.Application.Json)]
public class PlantsController : ControllerBase
{
    private readonly PlantKeeperDbContext _dbContext;
    private readonly IMapper _mapper;

    public PlantsController(PlantKeeperDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IEnumerable<PlantDto> List() => _mapper.Map<IEnumerable<PlantDto>>(
        _dbContext.Plants.OrderBy(plant => plant.Name)
    );

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create([FromBody] InputPlant plant)
    {
        var plantToCreate = _mapper.Map<Plant>(plant);
        await _dbContext.Plants.AddAsync(plantToCreate);
        await _dbContext.SaveChangesAsync();

        var plantToReturn = _mapper.Map<PlantDto>(plantToCreate);
        return CreatedAtAction(nameof(Get), new { plantId = plantToReturn.Id }, plantToReturn);
    }

    [HttpGet("{plantId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PlantDto>> Get([FromRoute] Guid plantId)
    {
        var plant = await _dbContext.Plants.FindAsync(plantId);
        return plant is not null
            ? Ok(_mapper.Map<PlantDto>(plant))
            : NotFound();
    }

    [HttpPut("{plantId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Update([FromRoute] Guid plantId, [FromBody] InputPlant plant)
    {
        var currentPlant = await _dbContext.Plants.FindAsync(plantId);
        if (currentPlant is null) return NotFound();

        _mapper.Map(plant, currentPlant);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{plantId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] Guid plantId)
    {
        var plant = await _dbContext.Plants.FindAsync(plantId);
        if (plant is null) return NotFound();

        _dbContext.Remove(plant);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }
}
