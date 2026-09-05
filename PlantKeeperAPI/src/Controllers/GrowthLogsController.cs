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
[Route("/api/growth-logs")]
[Consumes(MediaTypeNames.Application.Json)]
[Produces(MediaTypeNames.Application.Json)]
[RequiresPermission(Permissions.PlantsRead)]
public class GrowthLogsController : ControllerBase
{
    private readonly PlantKeeperDbContext _dbContext;
    private readonly IMapper _mapper;

    public GrowthLogsController(PlantKeeperDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    /// <param name="plantId">Optional - restricts the result to a single plant.</param>
    [HttpGet]
    [ProducesResponseType<IEnumerable<GrowthLogDto>>(StatusCodes.Status200OK)]
    public IEnumerable<GrowthLogDto> List([FromQuery] Guid? plantId)
    {
        IQueryable<GrowthLog> logs = _dbContext.GrowthLogs;
        if (plantId is not null) logs = logs.Where(log => log.PlantId == plantId);

        return _mapper.Map<IEnumerable<GrowthLogDto>>(logs.OrderBy(log => log.Date));
    }

    [RequiresPermission(Permissions.PlantsWrite)]
    [HttpPost]
    [ProducesResponseType<GrowthLogDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async ValueTask<ActionResult<GrowthLogDto>> Create([FromBody] InputGrowthLog log)
    {
        if (!await ReferencesResolveAsync(log))
            return UnprocessableEntity(new ValidationProblemDetails(ModelState));

        var logToCreate = _mapper.Map<GrowthLog>(log);
        await _dbContext.GrowthLogs.AddAsync(logToCreate);
        await _dbContext.SaveChangesAsync();

        var logToReturn = _mapper.Map<GrowthLogDto>(logToCreate);
        return CreatedAtAction(nameof(Get), new { growthLogId = logToReturn.Id }, logToReturn);
    }

    [HttpGet("{growthLogId:guid}")]
    [ProducesResponseType<GrowthLogDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async ValueTask<ActionResult<GrowthLogDto>> Get([FromRoute] Guid growthLogId)
    {
        GrowthLog? log = await _dbContext.GrowthLogs.FindAsync(growthLogId);
        return log is not null
            ? Ok(_mapper.Map<GrowthLogDto>(log))
            : NotFound();
    }

    [RequiresPermission(Permissions.PlantsWrite)]
    [HttpPut("{growthLogId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async ValueTask<IActionResult> Update([FromRoute] Guid growthLogId, [FromBody] InputGrowthLog log)
    {
        GrowthLog? currentLog = await _dbContext.GrowthLogs.FindAsync(growthLogId);
        if (currentLog is null) return NotFound();

        if (!await ReferencesResolveAsync(log))
            return UnprocessableEntity(new ValidationProblemDetails(ModelState));

        _mapper.Map(log, currentLog);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }

    [RequiresPermission(Permissions.PlantsWrite)]
    [HttpDelete("{growthLogId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async ValueTask<IActionResult> Delete([FromRoute] Guid growthLogId)
    {
        GrowthLog? log = await _dbContext.GrowthLogs.FindAsync(growthLogId);
        if (log is null) return NotFound();

        return await this.DeleteAsync(_dbContext, log) ?? NoContent();
    }

    private async ValueTask<bool> ReferencesResolveAsync(InputGrowthLog log)
    {
        await ModelState.RequireExistsAsync<Plant>(_dbContext, log.PlantId, "plantId");
        return ModelState.IsValid;
    }
}
