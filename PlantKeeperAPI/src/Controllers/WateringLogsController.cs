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
[Route("/api/watering-logs")]
[Consumes(MediaTypeNames.Application.Json)]
[Produces(MediaTypeNames.Application.Json)]
[RequiresPermission(Permissions.PlantsRead)]
public class WateringLogsController : ControllerBase
{
    private readonly PlantKeeperDbContext _dbContext;
    private readonly IMapper _mapper;

    public WateringLogsController(PlantKeeperDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    /// <param name="plantId">Optional - restricts the result to a single plant.</param>
    [HttpGet]
    [ProducesResponseType<IEnumerable<WateringLogDto>>(StatusCodes.Status200OK)]
    public IEnumerable<WateringLogDto> List([FromQuery] Guid? plantId)
    {
        IQueryable<WateringLog> logs = _dbContext.WateringLogs;
        if (plantId is not null) logs = logs.Where(log => log.PlantId == plantId);

        return _mapper.Map<IEnumerable<WateringLogDto>>(logs.OrderBy(log => log.Date));
    }

    [RequiresPermission(Permissions.PlantsWrite)]
    [HttpPost]
    [ProducesResponseType<WateringLogDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async ValueTask<ActionResult<WateringLogDto>> Create([FromBody] InputWateringLog log)
    {
        if (!await ReferencesResolveAsync(log))
            return UnprocessableEntity(new ValidationProblemDetails(ModelState));

        var logToCreate = _mapper.Map<WateringLog>(log);
        await _dbContext.WateringLogs.AddAsync(logToCreate);
        await _dbContext.SaveChangesAsync();

        var logToReturn = _mapper.Map<WateringLogDto>(logToCreate);
        return CreatedAtAction(nameof(Get), new { wateringLogId = logToReturn.Id }, logToReturn);
    }

    [HttpGet("{wateringLogId:guid}")]
    [ProducesResponseType<WateringLogDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async ValueTask<ActionResult<WateringLogDto>> Get([FromRoute] Guid wateringLogId)
    {
        WateringLog? log = await _dbContext.WateringLogs.FindAsync(wateringLogId);
        return log is not null
            ? Ok(_mapper.Map<WateringLogDto>(log))
            : NotFound();
    }

    [RequiresPermission(Permissions.PlantsWrite)]
    [HttpPut("{wateringLogId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async ValueTask<IActionResult> Update([FromRoute] Guid wateringLogId, [FromBody] InputWateringLog log)
    {
        WateringLog? currentLog = await _dbContext.WateringLogs.FindAsync(wateringLogId);
        if (currentLog is null) return NotFound();

        if (!await ReferencesResolveAsync(log))
            return UnprocessableEntity(new ValidationProblemDetails(ModelState));

        _mapper.Map(log, currentLog);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }

    [RequiresPermission(Permissions.PlantsWrite)]
    [HttpDelete("{wateringLogId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async ValueTask<IActionResult> Delete([FromRoute] Guid wateringLogId)
    {
        WateringLog? log = await _dbContext.WateringLogs.FindAsync(wateringLogId);
        if (log is null) return NotFound();

        return await this.DeleteAsync(_dbContext, log) ?? NoContent();
    }

    private async ValueTask<bool> ReferencesResolveAsync(InputWateringLog log)
    {
        await ModelState.RequireExistsAsync<Plant>(_dbContext, log.PlantId, "plantId");
        await ModelState.RequireExistsAsync<WateringMethod>(_dbContext, log.WateringMethodId, "wateringMethodId");
        return ModelState.IsValid;
    }
}
