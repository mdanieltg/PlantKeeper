using System.Net.Mime;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PlantKeeperAPI.Database;
using PlantKeeperAPI.DataTransferObjects;
using PlantKeeperAPI.Entities;
using PlantKeeperAPI.Models;

namespace PlantKeeperAPI.Controllers;

[ApiController]
[Route("/api/keepers")]
[Produces(MediaTypeNames.Application.Json)]
public class KeepersController : ControllerBase
{
    private readonly PlantKeeperDbContext _dbContext;
    private readonly IMapper _mapper;

    public KeepersController(PlantKeeperDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IEnumerable<KeeperDto> List() => _mapper.Map<IEnumerable<KeeperDto>>(_dbContext.Keepers);

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create([FromBody] InputKeeper keeper)
    {
        var keeperToCreate = new Keeper
        {
            Id = Guid.NewGuid(),
            Name = keeper.Name
        };
        await _dbContext.Keepers.AddAsync(keeperToCreate);
        await _dbContext.SaveChangesAsync();

        var keeperToReturn = _mapper.Map<KeeperDto>(keeperToCreate);
        return CreatedAtAction(nameof(Get), new { keeperId = keeperToReturn.Id }, keeperToReturn);
    }

    [HttpGet("{keeperId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<KeeperDto>> Get([FromRoute] Guid keeperId)
    {
        var keeper = await _dbContext.Keepers.FindAsync(keeperId);
        return keeper is not null
            ? Ok(_mapper.Map<KeeperDto>(keeper))
            : NotFound();
    }

    [HttpPut("{keeperId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Update([FromRoute] Guid keeperId, [FromBody] InputKeeper keeper)
    {
        var currentKeeper = await _dbContext.Keepers.FindAsync(keeperId);
        if (currentKeeper is null) return NotFound();

        currentKeeper.Name = keeper.Name;
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{keeperId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] Guid keeperId)
    {
        var keeper = await _dbContext.Keepers.FindAsync(keeperId);
        if (keeper is null) return NotFound();

        _dbContext.Remove(keeper);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }
}
