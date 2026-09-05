using System.Net.Mime;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlantKeeperAPI.Authorization;
using PlantKeeperAPI.Database;
using PlantKeeperAPI.DataTransferObjects;
using PlantKeeperAPI.Entities;
using PlantKeeperAPI.Models;

namespace PlantKeeperAPI.Controllers;

/// <summary>
/// One cell of the almanac fertilization matrix. Nested under the species, which owns every row and supplies the key the
/// body never carries. The table has a unique index on species and fertilizer category, so a duplicate is a
/// 409 rather than the 500 a raw <see cref="DbUpdateException" /> would produce.
/// </summary>
[ApiController]
[Route("/api/plant-species/{speciesId:guid}/fertilizer-recommendations")]
[Consumes(MediaTypeNames.Application.Json)]
[Produces(MediaTypeNames.Application.Json)]
[RequiresPermission(Permissions.AlmanacRead)]
public class SpeciesFertilizerRecommendationsController : ControllerBase
{
    private readonly PlantKeeperDbContext _dbContext;
    private readonly IMapper _mapper;

    public SpeciesFertilizerRecommendationsController(PlantKeeperDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    [HttpGet]
    [ProducesResponseType<IEnumerable<SpeciesFertilizerRecommendationDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async ValueTask<ActionResult<IEnumerable<SpeciesFertilizerRecommendationDto>>> List(
        [FromRoute] Guid speciesId)
    {
        if (!await SpeciesExistsAsync(speciesId)) return NotFound();

        List<SpeciesFertilizerRecommendation> rows = await _dbContext.SpeciesFertilizerRecommendations
            .AsNoTracking()
            .Where(row => row.SpeciesId == speciesId)
            .ToListAsync();

        return Ok(_mapper.Map<IEnumerable<SpeciesFertilizerRecommendationDto>>(rows));
    }

    [RequiresPermission(Permissions.AlmanacPropose)]
    [HttpPost]
    [ProducesResponseType<SpeciesFertilizerRecommendationDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async ValueTask<ActionResult<SpeciesFertilizerRecommendationDto>> Create([FromRoute] Guid speciesId,
        [FromBody] InputSpeciesFertilizerRecommendation input)
    {
        if (!await SpeciesExistsAsync(speciesId)) return NotFound();

        if (!await ReferencesResolveAsync(input))
            return UnprocessableEntity(new ValidationProblemDetails(ModelState));

        if (await CollidesAsync(speciesId, input, null)) return DuplicateCell();

        var rowToCreate = _mapper.Map<SpeciesFertilizerRecommendation>(input);
        rowToCreate.SpeciesId = speciesId;
        await _dbContext.SpeciesFertilizerRecommendations.AddAsync(rowToCreate);
        await _dbContext.SaveChangesAsync();

        var rowToReturn = _mapper.Map<SpeciesFertilizerRecommendationDto>(rowToCreate);
        return CreatedAtAction(nameof(Get), new { speciesId, recommendationId = rowToReturn.Id }, rowToReturn);
    }

    [HttpGet("{recommendationId:guid}")]
    [ProducesResponseType<SpeciesFertilizerRecommendationDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async ValueTask<ActionResult<SpeciesFertilizerRecommendationDto>> Get([FromRoute] Guid speciesId,
        [FromRoute] Guid recommendationId)
    {
        SpeciesFertilizerRecommendation? row = await FindAsync(speciesId, recommendationId);
        return row is not null
            ? Ok(_mapper.Map<SpeciesFertilizerRecommendationDto>(row))
            : NotFound();
    }

    [RequiresPermission(Permissions.AlmanacPropose)]
    [HttpPut("{recommendationId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async ValueTask<IActionResult> Update([FromRoute] Guid speciesId, [FromRoute] Guid recommendationId,
        [FromBody] InputSpeciesFertilizerRecommendation input)
    {
        SpeciesFertilizerRecommendation? currentRow = await FindAsync(speciesId, recommendationId);
        if (currentRow is null) return NotFound();

        if (!await ReferencesResolveAsync(input))
            return UnprocessableEntity(new ValidationProblemDetails(ModelState));

        if (await CollidesAsync(speciesId, input, recommendationId)) return DuplicateCell();

        _mapper.Map(input, currentRow);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }

    [RequiresPermission(Permissions.AlmanacPropose)]
    [HttpDelete("{recommendationId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async ValueTask<IActionResult> Delete([FromRoute] Guid speciesId, [FromRoute] Guid recommendationId)
    {
        SpeciesFertilizerRecommendation? row = await FindAsync(speciesId, recommendationId);
        if (row is null) return NotFound();

        _dbContext.Remove(row);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }

    private async ValueTask<bool> SpeciesExistsAsync(Guid speciesId) =>
        await _dbContext.PlantSpecies.AnyAsync(species => species.Id == speciesId);

    private async ValueTask<SpeciesFertilizerRecommendation?> FindAsync(Guid speciesId, Guid recommendationId) =>
        await _dbContext.SpeciesFertilizerRecommendations
            .FirstOrDefaultAsync(row => row.Id == recommendationId && row.SpeciesId == speciesId);

    private async ValueTask<bool> CollidesAsync(Guid speciesId, InputSpeciesFertilizerRecommendation input,
        Guid? excluding) =>
        await _dbContext.SpeciesFertilizerRecommendations
            .AnyAsync(row => row.SpeciesId == speciesId
                             && row.Category == input.Category
                             && (excluding == null || row.Id != excluding));

    private async ValueTask<bool> ReferencesResolveAsync(InputSpeciesFertilizerRecommendation input)
    {
        return await Task.FromResult(ModelState.IsValid);
    }

    private ConflictObjectResult DuplicateCell() => Conflict(new ProblemDetails
    {
        Status = StatusCodes.Status409Conflict,
        Title = "Duplicate matrix cell",
        Detail = "This species already has a row for that fertilizer category."
    });
}
