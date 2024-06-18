using System.Net.Mime;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PlantKeeperAPI.Database;
using PlantKeeperAPI.DataTransferObjects;
using PlantKeeperAPI.Entities;
using PlantKeeperAPI.Models;

namespace PlantKeeperAPI.Controllers;

[ApiController]
[Route("/api/watering-methods")]
[Produces(MediaTypeNames.Application.Json)]
public class WateringMethodsController : ControllerBase
{
    private readonly PlantKeeperDbContext _dbContext;
    private readonly IMapper _mapper;

    public WateringMethodsController(PlantKeeperDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IEnumerable<WateringMethodDto> List() =>
        _mapper.Map<IEnumerable<WateringMethodDto>>(_dbContext.WateringMethods);

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create([FromBody] InputWateringMethod wateringMethod)
    {
        var wateringMethodToCreate = _mapper.Map<WateringMethod>(wateringMethod);
        await _dbContext.WateringMethods.AddAsync(wateringMethodToCreate);
        await _dbContext.SaveChangesAsync();

        var wateringMethodToReturn = _mapper.Map<WateringMethodDto>(wateringMethodToCreate);
        return CreatedAtAction(nameof(Get), new { wateringMethodId = wateringMethodToReturn.Id },
            wateringMethodToReturn);
    }

    [HttpGet("{wateringMethodId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WateringMethodDto>> Get([FromRoute] Guid wateringMethodId)
    {
        var wateringMethod = await _dbContext.WateringMethods.FindAsync(wateringMethodId);
        return wateringMethod is not null
            ? Ok(_mapper.Map<WateringMethodDto>(wateringMethod))
            : NotFound();
    }

    [HttpPut("{wateringMethodId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Update([FromRoute] Guid wateringMethodId,
        [FromBody] InputWateringMethod wateringMethod)
    {
        var currentWateringMethod = await _dbContext.WateringMethods.FindAsync(wateringMethodId);
        if (currentWateringMethod is null) return NotFound();

        _mapper.Map(wateringMethod, currentWateringMethod);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{wateringMethodId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] Guid wateringMethodId)
    {
        var wateringMethod = await _dbContext.WateringMethods.FindAsync(wateringMethodId);
        if (wateringMethod is null) return NotFound();

        _dbContext.Remove(wateringMethod);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }
}
