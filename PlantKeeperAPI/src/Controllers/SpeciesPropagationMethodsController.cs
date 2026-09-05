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
/// One row of the almanac propagation matrix. Nested under the species, which owns every row and supplies the key the
/// body never carries. The table has a unique index on species and propagation method, so a duplicate is a
/// 409 rather than the 500 a raw <see cref="DbUpdateException" /> would produce.
/// </summary>
[ApiController]
[Route("/api/plant-species/{speciesId:guid}/propagation-methods")]
[Consumes(MediaTypeNames.Application.Json)]
[Produces(MediaTypeNames.Application.Json)]
[RequiresPermission(Permissions.AlmanacRead)]
public class SpeciesPropagationMethodsController : ControllerBase
{
    private readonly IAlmanacProposalService _almanac;
    private readonly PlantKeeperDbContext _dbContext;
    private readonly IMapper _mapper;

    public SpeciesPropagationMethodsController(PlantKeeperDbContext dbContext, IMapper mapper,
        IAlmanacProposalService almanac)
    {
        _dbContext = dbContext;
        _almanac = almanac;
        _mapper = mapper;
    }

    [HttpGet]
    [ProducesResponseType<IEnumerable<SpeciesPropagationMethodDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async ValueTask<ActionResult<IEnumerable<SpeciesPropagationMethodDto>>> List([FromRoute] Guid speciesId)
    {
        if (!await SpeciesExistsAsync(speciesId)) return NotFound();

        List<SpeciesPropagationMethod> rows = await _dbContext.SpeciesPropagationMethods
            .AsNoTracking()
            .Where(row => row.SpeciesId == speciesId)
            .ToListAsync();

        return Ok(_mapper.Map<IEnumerable<SpeciesPropagationMethodDto>>(rows));
    }

    [RequiresPermission(Permissions.AlmanacPropose)]
    [HttpPost]
    [ProducesResponseType<SpeciesPropagationMethodDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType<AlmanacChangeProposalDto>(StatusCodes.Status202Accepted)]
    public async ValueTask<ActionResult<SpeciesPropagationMethodDto>> Create([FromRoute] Guid speciesId,
        [FromBody] InputSpeciesPropagationMethod input)
    {
        if (!await SpeciesExistsAsync(speciesId)) return NotFound();

        if (!await ReferencesResolveAsync(input))
            return UnprocessableEntity(new ValidationProblemDetails(ModelState));

        if (await CollidesAsync(speciesId, input, null)) return DuplicateCell();

        if (await _almanac.SubmitAsync(AlmanacTargets.SpeciesPropagationMethod, AlmanacChangeOperation.Create,
                input, speciesId, null) is { } queued) return Accepted(queued);

        var rowToCreate = _mapper.Map<SpeciesPropagationMethod>(input);
        rowToCreate.SpeciesId = speciesId;
        await _dbContext.SpeciesPropagationMethods.AddAsync(rowToCreate);
        await _dbContext.SaveChangesAsync();

        var rowToReturn = _mapper.Map<SpeciesPropagationMethodDto>(rowToCreate);
        return CreatedAtAction(nameof(Get), new { speciesId, linkId = rowToReturn.Id }, rowToReturn);
    }

    [HttpGet("{linkId:guid}")]
    [ProducesResponseType<SpeciesPropagationMethodDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async ValueTask<ActionResult<SpeciesPropagationMethodDto>> Get([FromRoute] Guid speciesId,
        [FromRoute] Guid linkId)
    {
        SpeciesPropagationMethod? row = await FindAsync(speciesId, linkId);
        return row is not null
            ? Ok(_mapper.Map<SpeciesPropagationMethodDto>(row))
            : NotFound();
    }

    [RequiresPermission(Permissions.AlmanacPropose)]
    [HttpPut("{linkId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType<AlmanacChangeProposalDto>(StatusCodes.Status202Accepted)]
    public async ValueTask<IActionResult> Update([FromRoute] Guid speciesId, [FromRoute] Guid linkId,
        [FromBody] InputSpeciesPropagationMethod input)
    {
        SpeciesPropagationMethod? currentRow = await FindAsync(speciesId, linkId);
        if (currentRow is null) return NotFound();

        if (!await ReferencesResolveAsync(input))
            return UnprocessableEntity(new ValidationProblemDetails(ModelState));

        if (await CollidesAsync(speciesId, input, linkId)) return DuplicateCell();

        if (await _almanac.SubmitAsync(AlmanacTargets.SpeciesPropagationMethod, AlmanacChangeOperation.Update,
                input, currentRow.Id, currentRow.Version) is { } queued) return Accepted(queued);

        _mapper.Map(input, currentRow);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }

    [RequiresPermission(Permissions.AlmanacPropose)]
    [HttpDelete("{linkId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<AlmanacChangeProposalDto>(StatusCodes.Status202Accepted)]
    public async ValueTask<IActionResult> Delete([FromRoute] Guid speciesId, [FromRoute] Guid linkId)
    {
        SpeciesPropagationMethod? row = await FindAsync(speciesId, linkId);
        if (row is null) return NotFound();

        if (await _almanac.SubmitAsync(AlmanacTargets.SpeciesPropagationMethod, AlmanacChangeOperation.Delete,
                null, row.Id, row.Version) is { } queued) return Accepted(queued);

        return await this.DeleteAsync(_dbContext, row) ?? NoContent();
    }

    private async ValueTask<bool> SpeciesExistsAsync(Guid speciesId) =>
        await _dbContext.PlantSpecies.AnyAsync(species => species.Id == speciesId);

    private async ValueTask<SpeciesPropagationMethod?> FindAsync(Guid speciesId, Guid linkId) =>
        await _dbContext.SpeciesPropagationMethods
            .FirstOrDefaultAsync(row => row.Id == linkId && row.SpeciesId == speciesId);

    private async ValueTask<bool> CollidesAsync(Guid speciesId, InputSpeciesPropagationMethod input, Guid? excluding) =>
        await _dbContext.SpeciesPropagationMethods
            .AnyAsync(row => row.SpeciesId == speciesId
                             && row.PropagationMethodId == input.PropagationMethodId
                             && (excluding == null || row.Id != excluding));

    private async ValueTask<bool> ReferencesResolveAsync(InputSpeciesPropagationMethod input)
    {
        await ModelState.RequireExistsAsync<PropagationMethod>(_dbContext, input.PropagationMethodId,
            "propagationMethodId");
        return ModelState.IsValid;
    }

    private ConflictObjectResult DuplicateCell() => Conflict(new ProblemDetails
    {
        Status = StatusCodes.Status409Conflict,
        Title = "Duplicate matrix cell",
        Detail = "This species already has a row for that propagation method."
    });
}
