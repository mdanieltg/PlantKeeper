using System.Net.Mime;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using PlantKeeperAPI.Database;
using PlantKeeperAPI.DataTransferObjects;
using PlantKeeperAPI.Entities;
using PlantKeeperAPI.Models;

namespace PlantKeeperAPI.Controllers;

[ApiController]
[Route("/api/treatments")]
[Consumes(MediaTypeNames.Application.Json)]
[Produces(MediaTypeNames.Application.Json)]
public class TreatmentsController : ControllerBase
{
    private readonly PlantKeeperDbContext _dbContext;
    private readonly IMapper _mapper;

    public TreatmentsController(PlantKeeperDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    [HttpGet]
    [ProducesResponseType<IEnumerable<TreatmentDto>>(StatusCodes.Status200OK)]
    public IEnumerable<TreatmentDto> List() => _mapper.Map<IEnumerable<TreatmentDto>>(
        _dbContext.Treatments.OrderBy(treatment => treatment.Name)
    );

    [HttpPost]
    [ProducesResponseType<TreatmentDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async ValueTask<ActionResult<TreatmentDto>> Create([FromBody] InputTreatment treatment)
    {
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

    [HttpPut("{treatmentId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async ValueTask<IActionResult> Update([FromRoute] Guid treatmentId, [FromBody] InputTreatment treatment)
    {
        Treatment? currentTreatment = await _dbContext.Treatments.FindAsync(treatmentId);
        if (currentTreatment is null) return NotFound();

        _mapper.Map(treatment, currentTreatment);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{treatmentId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async ValueTask<IActionResult> Delete([FromRoute] Guid treatmentId)
    {
        Treatment? treatment = await _dbContext.Treatments.FindAsync(treatmentId);
        if (treatment is null) return NotFound();

        _dbContext.Remove(treatment);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }
}
