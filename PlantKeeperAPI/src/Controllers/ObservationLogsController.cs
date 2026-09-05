using System.Net.Mime;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using PlantKeeperAPI.Authorization;
using PlantKeeperAPI.Database;
using PlantKeeperAPI.DataTransferObjects;
using PlantKeeperAPI.Entities;
using PlantKeeperAPI.Extensions;
using PlantKeeperAPI.Models;

namespace PlantKeeperAPI.Controllers;

[ApiController]
[Route("/api/observation-logs")]
[Consumes(MediaTypeNames.Application.Json)]
[Produces(MediaTypeNames.Application.Json)]
[RequiresPermission(Permissions.PlantsRead)]
public class ObservationLogsController : ControllerBase
{
    private readonly PlantKeeperDbContext _dbContext;
    private readonly IMapper _mapper;

    public ObservationLogsController(PlantKeeperDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    /// <param name="plantId">Optional - restricts the result to a single plant.</param>
    [HttpGet]
    [ProducesResponseType<IEnumerable<ObservationLogDto>>(StatusCodes.Status200OK)]
    public IEnumerable<ObservationLogDto> List([FromQuery] Guid? plantId)
    {
        IQueryable<ObservationLog> logs = _dbContext.ObservationLogs;
        if (plantId is not null) logs = logs.Where(log => log.PlantId == plantId);

        return _mapper.Map<IEnumerable<ObservationLogDto>>(logs.OrderBy(log => log.Date));
    }

    [RequiresPermission(Permissions.PlantsWrite)]
    [HttpPost]
    [ProducesResponseType<ObservationLogDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async ValueTask<ActionResult<ObservationLogDto>> Create([FromBody] InputObservationLog log)
    {
        if (!await ReferencesResolveAsync(log))
            return UnprocessableEntity(new ValidationProblemDetails(ModelState));

        var logToCreate = _mapper.Map<ObservationLog>(log);
        await _dbContext.ObservationLogs.AddAsync(logToCreate);
        await _dbContext.SaveChangesAsync();

        var logToReturn = _mapper.Map<ObservationLogDto>(logToCreate);
        return CreatedAtAction(nameof(Get), new { observationLogId = logToReturn.Id }, logToReturn);
    }

    [HttpGet("{observationLogId:guid}")]
    [ProducesResponseType<ObservationLogDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async ValueTask<ActionResult<ObservationLogDto>> Get([FromRoute] Guid observationLogId)
    {
        ObservationLog? log = await _dbContext.ObservationLogs.FindAsync(observationLogId);
        return log is not null
            ? Ok(_mapper.Map<ObservationLogDto>(log))
            : NotFound();
    }

    [RequiresPermission(Permissions.PlantsWrite)]
    [HttpPut("{observationLogId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async ValueTask<IActionResult> Update([FromRoute] Guid observationLogId, [FromBody] InputObservationLog log)
    {
        ObservationLog? currentLog = await _dbContext.ObservationLogs.FindAsync(observationLogId);
        if (currentLog is null) return NotFound();

        if (!await ReferencesResolveAsync(log))
            return UnprocessableEntity(new ValidationProblemDetails(ModelState));

        _mapper.Map(log, currentLog);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }

    [RequiresPermission(Permissions.PlantsWrite)]
    [HttpDelete("{observationLogId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async ValueTask<IActionResult> Delete([FromRoute] Guid observationLogId)
    {
        ObservationLog? log = await _dbContext.ObservationLogs.FindAsync(observationLogId);
        if (log is null) return NotFound();

        _dbContext.Remove(log);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }

    private async ValueTask<bool> ReferencesResolveAsync(InputObservationLog log)
    {
        await ModelState.RequireExistsAsync<Plant>(_dbContext, log.PlantId, "plantId");
        return ModelState.IsValid;
    }
}
