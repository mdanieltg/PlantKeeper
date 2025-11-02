using System.Net.Mime;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using PlantKeeperAPI.Database;
using PlantKeeperAPI.DataTransferObjects;
using PlantKeeperAPI.Entities;
using PlantKeeperAPI.Models;

namespace PlantKeeperAPI.Controllers;

[ApiController]
[Route("/api/watering-logs")]
[Produces(MediaTypeNames.Application.Json)]
public class WateringLogsController : ControllerBase
{
    private readonly PlantKeeperDbContext _dbContext;
    private readonly IMapper _mapper;

    public WateringLogsController(PlantKeeperDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IEnumerable<WateringLogDto> List() => _mapper.Map<IEnumerable<WateringLogDto>>(
        _dbContext.WateringLogs.OrderBy(log => log.Date)
    );

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create([FromBody] InputWateringLog wateringLog)
    {
        var plant = await _dbContext.Plants.FindAsync(wateringLog.PlantId);
        var wateringMethod = await _dbContext.WateringMethods.FindAsync(wateringLog.WateringMethodId);
        var keeper = await _dbContext.Keepers.FindAsync(wateringLog.KeeperId);

        if (plant is null)
            ModelState.TryAddModelError("plantId", "The plant provided does not exist.");

        if (wateringMethod is null)
            ModelState.TryAddModelError("wateringMethodId", "The watering method provided does not exist");

        if (keeper is null)
            ModelState.TryAddModelError("keeperId", "The keeper provided does not exist.");

        if (!ModelState.IsValid) return UnprocessableEntity(new ValidationProblemDetails(ModelState));

        var wateringLogToCreate = _mapper.Map<WateringLog>(wateringLog);
        await _dbContext.WateringLogs.AddAsync(wateringLogToCreate);
        await _dbContext.SaveChangesAsync();

        var wateringLogToReturn = _mapper.Map<WateringLogDto>(wateringLogToCreate);
        return CreatedAtAction(nameof(Get), new { wateringLogId = wateringLogToReturn.Id }, wateringLogToReturn);
    }

    [HttpGet("{wateringLogId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WateringLogDto>> Get([FromRoute] Guid wateringLogId)
    {
        var wateringLog = await _dbContext.WateringLogs.FindAsync(wateringLogId);
        return wateringLog is not null
            ? Ok(_mapper.Map<WateringLogDto>(wateringLog))
            : NotFound();
    }

    [HttpPut("{wateringLogId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Update([FromRoute] Guid wateringLogId, [FromBody] InputWateringLog wateringLog)
    {
        var currentWateringLog = await _dbContext.WateringLogs.FindAsync(wateringLogId);
        if (currentWateringLog is null) return NotFound();

        var plant = await _dbContext.Plants.FindAsync(wateringLog.PlantId);
        var wateringMethod = await _dbContext.WateringMethods.FindAsync(wateringLog.WateringMethodId);
        var keeper = await _dbContext.Keepers.FindAsync(wateringLog.KeeperId);

        if (plant is null)
            ModelState.TryAddModelError("plantId", "The plant provided does not exist.");

        if (wateringMethod is null)
            ModelState.TryAddModelError("wateringMethodId", "The watering method provided does not exist");

        if (keeper is null)
            ModelState.TryAddModelError("keeperId", "The keeper provided does not exist.");

        if (!ModelState.IsValid) return UnprocessableEntity(new ValidationProblemDetails(ModelState));

        _mapper.Map(wateringLog, currentWateringLog);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{wateringLogId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] Guid wateringLogId)
    {
        var wateringLog = await _dbContext.WateringLogs.FindAsync(wateringLogId);
        if (wateringLog is null) return NotFound();

        _dbContext.Remove(wateringLog);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }
}
