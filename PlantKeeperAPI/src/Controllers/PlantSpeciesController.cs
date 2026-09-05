using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using PlantKeeperAPI.Authorization;
using PlantKeeperAPI.Database;
using PlantKeeperAPI.DataTransferObjects;
using PlantKeeperAPI.Entities;
using PlantKeeperAPI.Extensions;
using PlantKeeperAPI.Models;
using PlantKeeperAPI.Services;

namespace PlantKeeperAPI.Controllers;

/// <summary>
/// Species are read and written as a whole aggregate, care and toxicity profiles included.
/// The nested routes under <c>/care</c>, <c>/toxicity</c> and <c>/flowering</c> exist for
/// targeted edits to one profile.
/// </summary>
[ApiController]
[Route("/api/plant-species")]
[Consumes(MediaTypeNames.Application.Json)]
[Produces(MediaTypeNames.Application.Json)]
[RequiresPermission(Permissions.AlmanacRead)]
public class PlantSpeciesController : ControllerBase
{
    private readonly PlantKeeperDbContext _dbContext;
    private readonly IPlantSpeciesService _species;

    public PlantSpeciesController(PlantKeeperDbContext dbContext, IPlantSpeciesService species)
    {
        _dbContext = dbContext;
        _species = species;
    }

    [HttpGet]
    [ProducesResponseType<IEnumerable<PlantSpeciesDto>>(StatusCodes.Status200OK)]
    public async ValueTask<IEnumerable<PlantSpeciesDto>> List() => await _species.ListAsync();

    [RequiresPermission(Permissions.AlmanacPropose)]
    [HttpPost]
    [ProducesResponseType<PlantSpeciesDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async ValueTask<ActionResult<PlantSpeciesDto>> Create([FromBody] InputPlantSpecies species)
    {
        if (!await ReferencesResolveAsync(species))
            return UnprocessableEntity(new ValidationProblemDetails(ModelState));

        SpeciesWriteResult result = await _species.CreateAsync(species);
        if (result.Status is SpeciesWriteStatus.FloweringConflict) return FloweringConflict();

        return CreatedAtAction(nameof(Get), new { speciesId = result.Species!.Id }, result.Species);
    }

    [HttpGet("{speciesId:guid}")]
    [ProducesResponseType<PlantSpeciesDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async ValueTask<ActionResult<PlantSpeciesDto>> Get([FromRoute] Guid speciesId)
    {
        PlantSpeciesDto? species = await _species.GetAsync(speciesId);
        return species is not null ? Ok(species) : NotFound();
    }

    [RequiresPermission(Permissions.AlmanacPropose)]
    [HttpPut("{speciesId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async ValueTask<IActionResult> Update([FromRoute] Guid speciesId, [FromBody] InputPlantSpecies species)
    {
        if (!await ReferencesResolveAsync(species))
            return UnprocessableEntity(new ValidationProblemDetails(ModelState));

        SpeciesWriteResult result = await _species.UpdateAsync(speciesId, species);

        return result.Status switch
        {
            SpeciesWriteStatus.NotFound => NotFound(),
            SpeciesWriteStatus.FloweringConflict => FloweringConflict(),
            _ => NoContent()
        };
    }

    [RequiresPermission(Permissions.AlmanacPropose)]
    [HttpDelete("{speciesId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async ValueTask<IActionResult> Delete([FromRoute] Guid speciesId) =>
        await _species.DeleteAsync(speciesId) ? NoContent() : NotFound();

    private async ValueTask<bool> ReferencesResolveAsync(InputPlantSpecies species)
    {
        await ModelState.RequireExistsAsync<Climate>(_dbContext, species.ClimateId, "climateId");
        await ModelState.RequireExistsAsync<PottingMix>(_dbContext, species.PottingMixId, "pottingMixId");

        return ModelState.IsValid;
    }

    private UnprocessableEntityObjectResult FloweringConflict()
    {
        ModelState.TryAddModelError("flowering",
            "A species whose flowering habit is DoesNotFlower cannot carry a flowering profile.");

        return UnprocessableEntity(new ValidationProblemDetails(ModelState));
    }
}
