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
[Route("/api/fertilization-logs")]
[Consumes(MediaTypeNames.Application.Json)]
[Produces(MediaTypeNames.Application.Json)]
public class FertilizationLogsController : ControllerBase
{
    private readonly PlantKeeperDbContext _dbContext;
    private readonly IMapper _mapper;

    public FertilizationLogsController(PlantKeeperDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    /// <param name="plantId">Optional - restricts the result to a single plant.</param>
    [HttpGet]
    [ProducesResponseType<IEnumerable<FertilizationLogDto>>(StatusCodes.Status200OK)]
    public IEnumerable<FertilizationLogDto> List([FromQuery] Guid? plantId)
    {
        IQueryable<FertilizationLog> logs = _dbContext.FertilizationLogs;
        if (plantId is not null) logs = logs.Where(log => log.PlantId == plantId);

        return _mapper.Map<IEnumerable<FertilizationLogDto>>(logs.OrderBy(log => log.Date));
    }

    [HttpPost]
    [ProducesResponseType<FertilizationLogDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async ValueTask<ActionResult<FertilizationLogDto>> Create([FromBody] InputFertilizationLog log)
    {
        if (!await ReferencesResolveAsync(log))
            return UnprocessableEntity(new ValidationProblemDetails(ModelState));

        var logToCreate = _mapper.Map<FertilizationLog>(log);
        await _dbContext.FertilizationLogs.AddAsync(logToCreate);
        await _dbContext.SaveChangesAsync();

        var logToReturn = _mapper.Map<FertilizationLogDto>(logToCreate);
        return CreatedAtAction(nameof(Get), new { fertilizationLogId = logToReturn.Id }, logToReturn);
    }

    [HttpGet("{fertilizationLogId:guid}")]
    [ProducesResponseType<FertilizationLogDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async ValueTask<ActionResult<FertilizationLogDto>> Get([FromRoute] Guid fertilizationLogId)
    {
        FertilizationLog? log = await _dbContext.FertilizationLogs.FindAsync(fertilizationLogId);
        return log is not null
            ? Ok(_mapper.Map<FertilizationLogDto>(log))
            : NotFound();
    }

    [HttpPut("{fertilizationLogId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async ValueTask<IActionResult> Update([FromRoute] Guid fertilizationLogId,
        [FromBody] InputFertilizationLog log)
    {
        FertilizationLog? currentLog = await _dbContext.FertilizationLogs.FindAsync(fertilizationLogId);
        if (currentLog is null) return NotFound();

        if (!await ReferencesResolveAsync(log))
            return UnprocessableEntity(new ValidationProblemDetails(ModelState));

        _mapper.Map(log, currentLog);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{fertilizationLogId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async ValueTask<IActionResult> Delete([FromRoute] Guid fertilizationLogId)
    {
        FertilizationLog? log = await _dbContext.FertilizationLogs.FindAsync(fertilizationLogId);
        if (log is null) return NotFound();

        _dbContext.Remove(log);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }

    private async ValueTask<bool> ReferencesResolveAsync(InputFertilizationLog log)
    {
        await ModelState.RequireExistsAsync<Plant>(_dbContext, log.PlantId, "plantId");
        await ModelState.RequireExistsAsync<Fertilizer>(_dbContext, log.FertilizerId, "fertilizerId");
        return ModelState.IsValid;
    }
}
