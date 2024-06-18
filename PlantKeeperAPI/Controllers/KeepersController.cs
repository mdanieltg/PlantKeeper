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

    [HttpGet("{keeperId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<KeeperDto>> Get([FromRoute] Guid keeperId)
    {
        var keeper = await _dbContext.Keepers.FindAsync(keeperId);
        return keeper is null
            ? NotFound()
            : Ok(_mapper.Map<KeeperDto>(keeper));
    }
}
