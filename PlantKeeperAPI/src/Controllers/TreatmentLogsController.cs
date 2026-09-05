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
[Route("/api/treatment-logs")]
[Consumes(MediaTypeNames.Application.Json)]
[Produces(MediaTypeNames.Application.Json)]
[RequiresPermission(Permissions.PlantsRead)]
public class TreatmentLogsController : ControllerBase
{
    private readonly PlantKeeperDbContext _dbContext;
    private readonly IMapper _mapper;

    public TreatmentLogsController(PlantKeeperDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    /// <param name="plantId">Optional - restricts the result to a single plant.</param>
    [HttpGet]
    [ProducesResponseType<IEnumerable<TreatmentLogDto>>(StatusCodes.Status200OK)]
    public IEnumerable<TreatmentLogDto> List([FromQuery] Guid? plantId)
    {
        IQueryable<TreatmentLog> logs = _dbContext.TreatmentLogs;
        if (plantId is not null) logs = logs.Where(log => log.PlantId == plantId);

        return _mapper.Map<IEnumerable<TreatmentLogDto>>(logs.OrderBy(log => log.Date));
    }

    [RequiresPermission(Permissions.PlantsWrite)]
    [HttpPost]
    [ProducesResponseType<TreatmentLogDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async ValueTask<ActionResult<TreatmentLogDto>> Create([FromBody] InputTreatmentLog log)
    {
        if (!await ReferencesResolveAsync(log))
            return UnprocessableEntity(new ValidationProblemDetails(ModelState));

        var logToCreate = _mapper.Map<TreatmentLog>(log);
        await _dbContext.TreatmentLogs.AddAsync(logToCreate);
        await _dbContext.SaveChangesAsync();

        var logToReturn = _mapper.Map<TreatmentLogDto>(logToCreate);
        return CreatedAtAction(nameof(Get), new { treatmentLogId = logToReturn.Id }, logToReturn);
    }

    [HttpGet("{treatmentLogId:guid}")]
    [ProducesResponseType<TreatmentLogDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async ValueTask<ActionResult<TreatmentLogDto>> Get([FromRoute] Guid treatmentLogId)
    {
        TreatmentLog? log = await _dbContext.TreatmentLogs.FindAsync(treatmentLogId);
        return log is not null
            ? Ok(_mapper.Map<TreatmentLogDto>(log))
            : NotFound();
    }

    [RequiresPermission(Permissions.PlantsWrite)]
    [HttpPut("{treatmentLogId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async ValueTask<IActionResult> Update([FromRoute] Guid treatmentLogId, [FromBody] InputTreatmentLog log)
    {
        TreatmentLog? currentLog = await _dbContext.TreatmentLogs.FindAsync(treatmentLogId);
        if (currentLog is null) return NotFound();

        if (!await ReferencesResolveAsync(log))
            return UnprocessableEntity(new ValidationProblemDetails(ModelState));

        _mapper.Map(log, currentLog);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }

    [RequiresPermission(Permissions.PlantsWrite)]
    [HttpDelete("{treatmentLogId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async ValueTask<IActionResult> Delete([FromRoute] Guid treatmentLogId)
    {
        TreatmentLog? log = await _dbContext.TreatmentLogs.FindAsync(treatmentLogId);
        if (log is null) return NotFound();

        return await this.DeleteAsync(_dbContext, log) ?? NoContent();
    }

    private async ValueTask<bool> ReferencesResolveAsync(InputTreatmentLog log)
    {
        await ModelState.RequireExistsAsync<Plant>(_dbContext, log.PlantId, "plantId");
        await ModelState.RequireExistsAsync<Treatment>(_dbContext, log.TreatmentId, "treatmentId");
        return ModelState.IsValid;
    }
}
