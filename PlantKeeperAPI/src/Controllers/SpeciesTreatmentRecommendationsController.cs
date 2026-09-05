using System.Net.Mime;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlantKeeperAPI.Authorization;
using PlantKeeperAPI.DataTransferObjects;
using PlantKeeperAPI.Database;
using PlantKeeperAPI.Entities;
using PlantKeeperAPI.Extensions;
using PlantKeeperAPI.Enums;
using PlantKeeperAPI.Services;
using PlantKeeperAPI.Models;

namespace PlantKeeperAPI.Controllers;

/// <summary>
/// One cell of the almanac pest-control matrix. Nested under the species, which owns every row and supplies the key the
/// body never carries. The table has a unique index on species and treatment, so a duplicate is a
/// 409 rather than the 500 a raw <see cref="DbUpdateException" /> would produce.
/// </summary>
[ApiController]
[Route("/api/plant-species/{speciesId:guid}/treatment-recommendations")]
[Consumes(MediaTypeNames.Application.Json)]
[Produces(MediaTypeNames.Application.Json)]
[RequiresPermission(Permissions.AlmanacRead)]
public class SpeciesTreatmentRecommendationsController : ControllerBase
{
    private readonly IAlmanacProposalService _almanac;
    private readonly PlantKeeperDbContext _dbContext;
    private readonly IMapper _mapper;

    public SpeciesTreatmentRecommendationsController(PlantKeeperDbContext dbContext, IMapper mapper,
        IAlmanacProposalService almanac)
    {
        _dbContext = dbContext;
        _almanac = almanac;
        _mapper = mapper;
    }

    [HttpGet]
    [ProducesResponseType<IEnumerable<SpeciesTreatmentRecommendationDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async ValueTask<ActionResult<IEnumerable<SpeciesTreatmentRecommendationDto>>> List(
        [FromRoute] Guid speciesId)
    {
        if (!await SpeciesExistsAsync(speciesId)) return NotFound();

        List<SpeciesTreatmentRecommendation> rows = await _dbContext.SpeciesTreatmentRecommendations
            .AsNoTracking()
            .Where(row => row.SpeciesId == speciesId)
            .ToListAsync();

        return Ok(_mapper.Map<IEnumerable<SpeciesTreatmentRecommendationDto>>(rows));
    }

    [RequiresPermission(Permissions.AlmanacPropose)]
    [HttpPost]
    [ProducesResponseType<SpeciesTreatmentRecommendationDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType<AlmanacChangeProposalDto>(StatusCodes.Status202Accepted)]
    public async ValueTask<ActionResult<SpeciesTreatmentRecommendationDto>> Create([FromRoute] Guid speciesId,
        [FromBody] InputSpeciesTreatmentRecommendation input)
    {
        if (!await SpeciesExistsAsync(speciesId)) return NotFound();

        if (!await ReferencesResolveAsync(input))
            return UnprocessableEntity(new ValidationProblemDetails(ModelState));

        if (await CollidesAsync(speciesId, input, null)) return DuplicateCell();

        if (await _almanac.SubmitAsync(AlmanacTargets.SpeciesTreatmentRecommendation, AlmanacChangeOperation.Create,
                input, speciesId, null) is { } queued) return Accepted(queued);

        var rowToCreate = _mapper.Map<SpeciesTreatmentRecommendation>(input);
        rowToCreate.SpeciesId = speciesId;
        await _dbContext.SpeciesTreatmentRecommendations.AddAsync(rowToCreate);
        await _dbContext.SaveChangesAsync();

        var rowToReturn = _mapper.Map<SpeciesTreatmentRecommendationDto>(rowToCreate);
        return CreatedAtAction(nameof(Get), new { speciesId, recommendationId = rowToReturn.Id }, rowToReturn);
    }

    [HttpGet("{recommendationId:guid}")]
    [ProducesResponseType<SpeciesTreatmentRecommendationDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async ValueTask<ActionResult<SpeciesTreatmentRecommendationDto>> Get([FromRoute] Guid speciesId,
        [FromRoute] Guid recommendationId)
    {
        SpeciesTreatmentRecommendation? row = await FindAsync(speciesId, recommendationId);
        return row is not null
            ? Ok(_mapper.Map<SpeciesTreatmentRecommendationDto>(row))
            : NotFound();
    }

    [RequiresPermission(Permissions.AlmanacPropose)]
    [HttpPut("{recommendationId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType<AlmanacChangeProposalDto>(StatusCodes.Status202Accepted)]
    public async ValueTask<IActionResult> Update([FromRoute] Guid speciesId, [FromRoute] Guid recommendationId,
        [FromBody] InputSpeciesTreatmentRecommendation input)
    {
        SpeciesTreatmentRecommendation? currentRow = await FindAsync(speciesId, recommendationId);
        if (currentRow is null) return NotFound();

        if (!await ReferencesResolveAsync(input))
            return UnprocessableEntity(new ValidationProblemDetails(ModelState));

        if (await CollidesAsync(speciesId, input, recommendationId)) return DuplicateCell();

        if (await _almanac.SubmitAsync(AlmanacTargets.SpeciesTreatmentRecommendation, AlmanacChangeOperation.Update,
                input, currentRow.Id, currentRow.Version) is { } queued) return Accepted(queued);

        _mapper.Map(input, currentRow);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }

    [RequiresPermission(Permissions.AlmanacPropose)]
    [HttpDelete("{recommendationId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<AlmanacChangeProposalDto>(StatusCodes.Status202Accepted)]
    public async ValueTask<IActionResult> Delete([FromRoute] Guid speciesId, [FromRoute] Guid recommendationId)
    {
        SpeciesTreatmentRecommendation? row = await FindAsync(speciesId, recommendationId);
        if (row is null) return NotFound();

        if (await _almanac.SubmitAsync(AlmanacTargets.SpeciesTreatmentRecommendation, AlmanacChangeOperation.Delete,
                null, row.Id, row.Version) is { } queued) return Accepted(queued);

        return await this.DeleteAsync(_dbContext, row) ?? NoContent();
    }

    private async ValueTask<bool> SpeciesExistsAsync(Guid speciesId) =>
        await _dbContext.PlantSpecies.AnyAsync(species => species.Id == speciesId);

    private async ValueTask<SpeciesTreatmentRecommendation?> FindAsync(Guid speciesId, Guid recommendationId) =>
        await _dbContext.SpeciesTreatmentRecommendations
            .FirstOrDefaultAsync(row => row.Id == recommendationId && row.SpeciesId == speciesId);

    private async ValueTask<bool> CollidesAsync(Guid speciesId, InputSpeciesTreatmentRecommendation input,
        Guid? excluding) =>
        await _dbContext.SpeciesTreatmentRecommendations
            .AnyAsync(row => row.SpeciesId == speciesId
                             && row.TreatmentId == input.TreatmentId
                             && (excluding == null || row.Id != excluding));

    private async ValueTask<bool> ReferencesResolveAsync(InputSpeciesTreatmentRecommendation input)
    {
        await ModelState.RequireExistsAsync<Treatment>(_dbContext, input.TreatmentId, "treatmentId");
        return ModelState.IsValid;
    }

    private ConflictObjectResult DuplicateCell() => Conflict(new ProblemDetails
    {
        Status = StatusCodes.Status409Conflict,
        Title = "Duplicate matrix cell",
        Detail = "This species already has a row for that treatment."
    });
}
