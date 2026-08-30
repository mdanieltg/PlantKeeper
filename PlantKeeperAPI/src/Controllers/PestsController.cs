using System.Net.Mime;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlantKeeperAPI.Database;
using PlantKeeperAPI.DataTransferObjects;
using PlantKeeperAPI.Entities;
using PlantKeeperAPI.Extensions;
using PlantKeeperAPI.Models;

namespace PlantKeeperAPI.Controllers;

[ApiController]
[Route("/api/pests")]
[Consumes(MediaTypeNames.Application.Json)]
[Produces(MediaTypeNames.Application.Json)]
public class PestsController : ControllerBase
{
    private readonly PlantKeeperDbContext _dbContext;
    private readonly IMapper _mapper;

    public PestsController(PlantKeeperDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    [HttpGet]
    [ProducesResponseType<IEnumerable<PestDto>>(StatusCodes.Status200OK)]
    public IEnumerable<PestDto> List() => _mapper.Map<IEnumerable<PestDto>>(
        _dbContext.Pests.OrderBy(pest => pest.Name)
    );

    [HttpPost]
    [ProducesResponseType<PestDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async ValueTask<ActionResult<PestDto>> Create([FromBody] InputPest pest)
    {
        var pestToCreate = _mapper.Map<Pest>(pest);
        await _dbContext.Pests.AddAsync(pestToCreate);
        await _dbContext.SaveChangesAsync();

        var pestToReturn = _mapper.Map<PestDto>(pestToCreate);
        return CreatedAtAction(nameof(Get), new { pestId = pestToReturn.Id }, pestToReturn);
    }

    [HttpGet("{pestId:guid}")]
    [ProducesResponseType<PestDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async ValueTask<ActionResult<PestDto>> Get([FromRoute] Guid pestId)
    {
        Pest? pest = await _dbContext.Pests.FindAsync(pestId);
        return pest is not null
            ? Ok(_mapper.Map<PestDto>(pest))
            : NotFound();
    }

    [HttpPut("{pestId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async ValueTask<IActionResult> Update([FromRoute] Guid pestId, [FromBody] InputPest pest)
    {
        Pest? currentPest = await _dbContext.Pests.FindAsync(pestId);
        if (currentPest is null) return NotFound();

        _mapper.Map(pest, currentPest);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{pestId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async ValueTask<IActionResult> Delete([FromRoute] Guid pestId)
    {
        Pest? pest = await _dbContext.Pests.FindAsync(pestId);
        if (pest is null) return NotFound();

        _dbContext.Remove(pest);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("{pestId:guid}/treatments")]
    [ProducesResponseType<IEnumerable<TreatmentDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async ValueTask<ActionResult<IEnumerable<TreatmentDto>>> ListTreatments([FromRoute] Guid pestId)
    {
        Pest? pest = await _dbContext.Pests
            .AsNoTracking()
            .Include(entry => entry.Treatments)
            .FirstOrDefaultAsync(entry => entry.Id == pestId);

        return pest is not null
            ? Ok(_mapper.Map<IEnumerable<TreatmentDto>>(pest.Treatments.OrderBy(treatment => treatment.Name)))
            : NotFound();
    }

    /// <summary>Replaces the whole set of treatments known to work on this pest.</summary>
    [HttpPut("{pestId:guid}/treatments")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async ValueTask<IActionResult> ReplaceTreatments([FromRoute] Guid pestId,
        [FromBody] IReadOnlyCollection<Guid> treatmentIds)
    {
        Pest? pest = await _dbContext.Pests
            .Include(entry => entry.Treatments)
            .FirstOrDefaultAsync(entry => entry.Id == pestId);

        if (pest is null) return NotFound();

        if (!await ModelState.RequireAllExistAsync<Treatment>(_dbContext, treatmentIds, "treatmentIds"))
            return UnprocessableEntity(new ValidationProblemDetails(ModelState));

        List<Treatment> treatments = await _dbContext.Treatments
            .Where(treatment => treatmentIds.Contains(treatment.Id))
            .ToListAsync();

        pest.Treatments.Clear();
        pest.Treatments.AddRange(treatments);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }
}
