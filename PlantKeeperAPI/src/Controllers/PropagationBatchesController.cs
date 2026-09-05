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
[Route("/api/propagation-batches")]
[Consumes(MediaTypeNames.Application.Json)]
[Produces(MediaTypeNames.Application.Json)]
[RequiresPermission(Permissions.PlantsRead)]
public class PropagationBatchesController : ControllerBase
{
    private readonly PlantKeeperDbContext _dbContext;
    private readonly IMapper _mapper;

    public PropagationBatchesController(PlantKeeperDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    /// <param name="speciesId">Optional - restricts the result to a single species.</param>
    [HttpGet]
    [ProducesResponseType<IEnumerable<PropagationBatchDto>>(StatusCodes.Status200OK)]
    public IEnumerable<PropagationBatchDto> List([FromQuery] Guid? speciesId)
    {
        IQueryable<PropagationBatch> batches = _dbContext.PropagationBatches;
        if (speciesId is not null) batches = batches.Where(batch => batch.SpeciesId == speciesId);

        return _mapper.Map<IEnumerable<PropagationBatchDto>>(batches.OrderBy(batch => batch.StartDate));
    }

    [RequiresPermission(Permissions.PlantsWrite)]
    [HttpPost]
    [ProducesResponseType<PropagationBatchDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async ValueTask<ActionResult<PropagationBatchDto>> Create([FromBody] InputPropagationBatch batch)
    {
        if (!await ReferencesResolveAsync(batch))
            return UnprocessableEntity(new ValidationProblemDetails(ModelState));

        var batchToCreate = _mapper.Map<PropagationBatch>(batch);
        await _dbContext.PropagationBatches.AddAsync(batchToCreate);
        await _dbContext.SaveChangesAsync();

        var batchToReturn = _mapper.Map<PropagationBatchDto>(batchToCreate);
        return CreatedAtAction(nameof(Get), new { propagationBatchId = batchToReturn.Id }, batchToReturn);
    }

    [HttpGet("{propagationBatchId:guid}")]
    [ProducesResponseType<PropagationBatchDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async ValueTask<ActionResult<PropagationBatchDto>> Get([FromRoute] Guid propagationBatchId)
    {
        PropagationBatch? batch = await _dbContext.PropagationBatches.FindAsync(propagationBatchId);
        return batch is not null
            ? Ok(_mapper.Map<PropagationBatchDto>(batch))
            : NotFound();
    }

    [RequiresPermission(Permissions.PlantsWrite)]
    [HttpPut("{propagationBatchId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async ValueTask<IActionResult> Update([FromRoute] Guid propagationBatchId,
        [FromBody] InputPropagationBatch batch)
    {
        PropagationBatch? currentBatch = await _dbContext.PropagationBatches.FindAsync(propagationBatchId);
        if (currentBatch is null) return NotFound();

        if (!await ReferencesResolveAsync(batch))
            return UnprocessableEntity(new ValidationProblemDetails(ModelState));

        _mapper.Map(batch, currentBatch);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }

    [RequiresPermission(Permissions.PlantsWrite)]
    [HttpDelete("{propagationBatchId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async ValueTask<IActionResult> Delete([FromRoute] Guid propagationBatchId)
    {
        PropagationBatch? batch = await _dbContext.PropagationBatches.FindAsync(propagationBatchId);
        if (batch is null) return NotFound();

        _dbContext.Remove(batch);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }

    private async ValueTask<bool> ReferencesResolveAsync(InputPropagationBatch batch)
    {
        await ModelState.RequireExistsAsync<PlantSpecies>(_dbContext, batch.SpeciesId, "speciesId");
        await ModelState.RequireExistsAsync<Plant>(_dbContext, batch.SourcePlantId, "sourcePlantId");
        await ModelState.RequireExistsAsync<PropagationMethod>(
            _dbContext, batch.PropagationMethodId, "propagationMethodId");

        return ModelState.IsValid;
    }
}
