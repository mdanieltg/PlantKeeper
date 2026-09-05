using System.Net.Mime;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using PlantKeeperAPI.Authorization;
using PlantKeeperAPI.DataTransferObjects;
using PlantKeeperAPI.Database;
using PlantKeeperAPI.Entities;
using PlantKeeperAPI.Extensions;
using PlantKeeperAPI.Models;

namespace PlantKeeperAPI.Controllers;

[ApiController]
[Route("/api/repotting-logs")]
[Consumes(MediaTypeNames.Application.Json)]
[Produces(MediaTypeNames.Application.Json)]
[RequiresPermission(Permissions.PlantsRead)]
public class RepottingLogsController : ControllerBase
{
    private readonly PlantKeeperDbContext _dbContext;
    private readonly IMapper _mapper;

    public RepottingLogsController(PlantKeeperDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    /// <param name="plantId">Optional - restricts the result to a single plant.</param>
    [HttpGet]
    [ProducesResponseType<IEnumerable<RepottingLogDto>>(StatusCodes.Status200OK)]
    public IEnumerable<RepottingLogDto> List([FromQuery] Guid? plantId)
    {
        IQueryable<RepottingLog> logs = _dbContext.RepottingLogs;
        if (plantId is not null) logs = logs.Where(log => log.PlantId == plantId);

        return _mapper.Map<IEnumerable<RepottingLogDto>>(logs.OrderBy(log => log.Date));
    }

    [RequiresPermission(Permissions.PlantsWrite)]
    [HttpPost]
    [ProducesResponseType<RepottingLogDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async ValueTask<ActionResult<RepottingLogDto>> Create([FromBody] InputRepottingLog log)
    {
        if (!await ReferencesResolveAsync(log))
            return UnprocessableEntity(new ValidationProblemDetails(ModelState));

        var logToCreate = _mapper.Map<RepottingLog>(log);
        await _dbContext.RepottingLogs.AddAsync(logToCreate);
        await _dbContext.SaveChangesAsync();

        var logToReturn = _mapper.Map<RepottingLogDto>(logToCreate);
        return CreatedAtAction(nameof(Get), new { repottingLogId = logToReturn.Id }, logToReturn);
    }

    [HttpGet("{repottingLogId:guid}")]
    [ProducesResponseType<RepottingLogDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async ValueTask<ActionResult<RepottingLogDto>> Get([FromRoute] Guid repottingLogId)
    {
        RepottingLog? log = await _dbContext.RepottingLogs.FindAsync(repottingLogId);
        return log is not null
            ? Ok(_mapper.Map<RepottingLogDto>(log))
            : NotFound();
    }

    [RequiresPermission(Permissions.PlantsWrite)]
    [HttpPut("{repottingLogId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async ValueTask<IActionResult> Update([FromRoute] Guid repottingLogId, [FromBody] InputRepottingLog log)
    {
        RepottingLog? currentLog = await _dbContext.RepottingLogs.FindAsync(repottingLogId);
        if (currentLog is null) return NotFound();

        if (!await ReferencesResolveAsync(log))
            return UnprocessableEntity(new ValidationProblemDetails(ModelState));

        _mapper.Map(log, currentLog);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }

    [RequiresPermission(Permissions.PlantsWrite)]
    [HttpDelete("{repottingLogId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async ValueTask<IActionResult> Delete([FromRoute] Guid repottingLogId)
    {
        RepottingLog? log = await _dbContext.RepottingLogs.FindAsync(repottingLogId);
        if (log is null) return NotFound();

        return await this.DeleteAsync(_dbContext, log) ?? NoContent();
    }

    private async ValueTask<bool> ReferencesResolveAsync(InputRepottingLog log)
    {
        await ModelState.RequireExistsAsync<Plant>(_dbContext, log.PlantId, "plantId");
        return ModelState.IsValid;
    }
}
