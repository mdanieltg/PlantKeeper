using System.Net.Mime;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using PlantKeeperAPI.Authorization;
using PlantKeeperAPI.DataTransferObjects;
using PlantKeeperAPI.Database;
using PlantKeeperAPI.Entities;
using PlantKeeperAPI.Enums;
using PlantKeeperAPI.Extensions;
using PlantKeeperAPI.Models;
using PlantKeeperAPI.Services;

namespace PlantKeeperAPI.Controllers;

[ApiController]
[Route("/api/treatments")]
[Consumes(MediaTypeNames.Application.Json)]
[Produces(MediaTypeNames.Application.Json)]
[RequiresPermission(Permissions.AlmanacRead)]
public class TreatmentsController : ControllerBase
{
    private readonly IAlmanacProposalService _almanac;
    private readonly PlantKeeperDbContext _dbContext;
    private readonly IMapper _mapper;

    public TreatmentsController(PlantKeeperDbContext dbContext, IMapper mapper,
        IAlmanacProposalService almanac)
    {
        _dbContext = dbContext;
        _almanac = almanac;
        _mapper = mapper;
    }

    [HttpGet]
    [ProducesResponseType<IEnumerable<TreatmentDto>>(StatusCodes.Status200OK)]
    public IEnumerable<TreatmentDto> List() => _mapper.Map<IEnumerable<TreatmentDto>>(
        _dbContext.Treatments.OrderBy(treatment => treatment.Name)
    );

    [RequiresPermission(Permissions.AlmanacPropose)]
    [HttpPost]
    [ProducesResponseType<TreatmentDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType<AlmanacChangeProposalDto>(StatusCodes.Status202Accepted)]
    public async ValueTask<ActionResult<TreatmentDto>> Create([FromBody] InputTreatment treatment)
    {
        if (await _almanac.SubmitAsync(AlmanacTargets.Treatment, AlmanacChangeOperation.Create,
                treatment) is { } queued) return Accepted(queued);

        var treatmentToCreate = _mapper.Map<Treatment>(treatment);
        await _dbContext.Treatments.AddAsync(treatmentToCreate);
        await _dbContext.SaveChangesAsync();

        var treatmentToReturn = _mapper.Map<TreatmentDto>(treatmentToCreate);
        return CreatedAtAction(nameof(Get), new { treatmentId = treatmentToReturn.Id }, treatmentToReturn);
    }

    [HttpGet("{treatmentId:guid}")]
    [ProducesResponseType<TreatmentDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async ValueTask<ActionResult<TreatmentDto>> Get([FromRoute] Guid treatmentId)
    {
        Treatment? treatment = await _dbContext.Treatments.FindAsync(treatmentId);
        return treatment is not null
            ? Ok(_mapper.Map<TreatmentDto>(treatment))
            : NotFound();
    }

    [RequiresPermission(Permissions.AlmanacPropose)]
    [HttpPut("{treatmentId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType<AlmanacChangeProposalDto>(StatusCodes.Status202Accepted)]
    public async ValueTask<IActionResult> Update([FromRoute] Guid treatmentId, [FromBody] InputTreatment treatment)
    {
        Treatment? currentTreatment = await _dbContext.Treatments.FindAsync(treatmentId);
        if (currentTreatment is null) return NotFound();

        if (await _almanac.SubmitAsync(AlmanacTargets.Treatment, AlmanacChangeOperation.Update,
                treatment, currentTreatment.Id, currentTreatment.Version) is { } queued) return Accepted(queued);

        _mapper.Map(treatment, currentTreatment);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }

    [RequiresPermission(Permissions.AlmanacPropose)]
    [HttpDelete("{treatmentId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<AlmanacChangeProposalDto>(StatusCodes.Status202Accepted)]
    public async ValueTask<IActionResult> Delete([FromRoute] Guid treatmentId)
    {
        Treatment? treatment = await _dbContext.Treatments.FindAsync(treatmentId);
        if (treatment is null) return NotFound();

        if (await _almanac.SubmitAsync(AlmanacTargets.Treatment, AlmanacChangeOperation.Delete,
                null, treatment.Id, treatment.Version) is { } queued) return Accepted(queued);

        return await this.DeleteAsync(_dbContext, treatment) ?? NoContent();
    }
}
