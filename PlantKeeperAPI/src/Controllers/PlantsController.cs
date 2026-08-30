using System.Net.Mime;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using PlantKeeperAPI.Database;
using PlantKeeperAPI.DataTransferObjects;
using PlantKeeperAPI.Entities;
using PlantKeeperAPI.Extensions;
using PlantKeeperAPI.Models;

namespace PlantKeeperAPI.Controllers;

[ApiController]
[Route("/api/plants")]
[Consumes(MediaTypeNames.Application.Json)]
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

    /// <param name="speciesId">Optional - restricts the result to a single species.</param>
    [HttpGet]
    [ProducesResponseType<IEnumerable<PlantDto>>(StatusCodes.Status200OK)]
    public IEnumerable<PlantDto> List([FromQuery] Guid? speciesId)
    {
        IQueryable<Plant> plants = _dbContext.Plants;
        if (speciesId is not null) plants = plants.Where(plant => plant.SpeciesId == speciesId);

        return _mapper.Map<IEnumerable<PlantDto>>(plants.OrderBy(plant => plant.Alias));
    }

    [HttpPost]
    [ProducesResponseType<PlantDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async ValueTask<ActionResult<PlantDto>> Create([FromBody] InputPlant plant)
    {
        if (!await ModelState.RequireExistsAsync<PlantSpecies>(_dbContext, plant.SpeciesId, "speciesId"))
            return UnprocessableEntity(new ValidationProblemDetails(ModelState));

        var plantToCreate = _mapper.Map<Plant>(plant);
        await _dbContext.Plants.AddAsync(plantToCreate);
        await _dbContext.SaveChangesAsync();

        var plantToReturn = _mapper.Map<PlantDto>(plantToCreate);
        return CreatedAtAction(nameof(Get), new { plantId = plantToReturn.Id }, plantToReturn);
    }

    [HttpGet("{plantId:guid}")]
    [ProducesResponseType<PlantDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async ValueTask<ActionResult<PlantDto>> Get([FromRoute] Guid plantId)
    {
        Plant? plant = await _dbContext.Plants.FindAsync(plantId);
        return plant is not null
            ? Ok(_mapper.Map<PlantDto>(plant))
            : NotFound();
    }

    [HttpPut("{plantId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async ValueTask<IActionResult> Update([FromRoute] Guid plantId, [FromBody] InputPlant plant)
    {
        Plant? currentPlant = await _dbContext.Plants.FindAsync(plantId);
        if (currentPlant is null) return NotFound();

        if (!await ModelState.RequireExistsAsync<PlantSpecies>(_dbContext, plant.SpeciesId, "speciesId"))
            return UnprocessableEntity(new ValidationProblemDetails(ModelState));

        _mapper.Map(plant, currentPlant);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{plantId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async ValueTask<IActionResult> Delete([FromRoute] Guid plantId)
    {
        Plant? plant = await _dbContext.Plants.FindAsync(plantId);
        if (plant is null) return NotFound();

        _dbContext.Remove(plant);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }
}
