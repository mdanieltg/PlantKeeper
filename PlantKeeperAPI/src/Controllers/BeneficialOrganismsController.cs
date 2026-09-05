using System.Net.Mime;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlantKeeperAPI.Authorization;
using PlantKeeperAPI.Database;
using PlantKeeperAPI.DataTransferObjects;
using PlantKeeperAPI.Entities;
using PlantKeeperAPI.Extensions;
using PlantKeeperAPI.Models;

namespace PlantKeeperAPI.Controllers;

[ApiController]
[Route("/api/beneficial-organisms")]
[Consumes(MediaTypeNames.Application.Json)]
[Produces(MediaTypeNames.Application.Json)]
[RequiresPermission(Permissions.AlmanacRead)]
public class BeneficialOrganismsController : ControllerBase
{
    private readonly PlantKeeperDbContext _dbContext;
    private readonly IMapper _mapper;

    public BeneficialOrganismsController(PlantKeeperDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    [HttpGet]
    [ProducesResponseType<IEnumerable<BeneficialOrganismDto>>(StatusCodes.Status200OK)]
    public IEnumerable<BeneficialOrganismDto> List() => _mapper.Map<IEnumerable<BeneficialOrganismDto>>(
        _dbContext.BeneficialOrganisms.OrderBy(organism => organism.Name)
    );

    [RequiresPermission(Permissions.AlmanacPropose)]
    [HttpPost]
    [ProducesResponseType<BeneficialOrganismDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async ValueTask<ActionResult<BeneficialOrganismDto>> Create([FromBody] InputBeneficialOrganism organism)
    {
        var organismToCreate = _mapper.Map<BeneficialOrganism>(organism);
        await _dbContext.BeneficialOrganisms.AddAsync(organismToCreate);
        await _dbContext.SaveChangesAsync();

        var organismToReturn = _mapper.Map<BeneficialOrganismDto>(organismToCreate);
        return CreatedAtAction(nameof(Get), new { organismId = organismToReturn.Id }, organismToReturn);
    }

    [HttpGet("{organismId:guid}")]
    [ProducesResponseType<BeneficialOrganismDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async ValueTask<ActionResult<BeneficialOrganismDto>> Get([FromRoute] Guid organismId)
    {
        BeneficialOrganism? organism = await _dbContext.BeneficialOrganisms.FindAsync(organismId);
        return organism is not null
            ? Ok(_mapper.Map<BeneficialOrganismDto>(organism))
            : NotFound();
    }

    [RequiresPermission(Permissions.AlmanacPropose)]
    [HttpPut("{organismId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async ValueTask<IActionResult> Update([FromRoute] Guid organismId,
        [FromBody] InputBeneficialOrganism organism)
    {
        BeneficialOrganism? currentOrganism = await _dbContext.BeneficialOrganisms.FindAsync(organismId);
        if (currentOrganism is null) return NotFound();

        _mapper.Map(organism, currentOrganism);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }

    [RequiresPermission(Permissions.AlmanacPropose)]
    [HttpDelete("{organismId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async ValueTask<IActionResult> Delete([FromRoute] Guid organismId)
    {
        BeneficialOrganism? organism = await _dbContext.BeneficialOrganisms.FindAsync(organismId);
        if (organism is null) return NotFound();

        _dbContext.Remove(organism);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("{organismId:guid}/species")]
    [ProducesResponseType<IEnumerable<PlantSpeciesDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async ValueTask<ActionResult<IEnumerable<PlantSpeciesDto>>> ListSpecies([FromRoute] Guid organismId)
    {
        BeneficialOrganism? organism = await _dbContext.BeneficialOrganisms
            .AsNoTracking()
            .Include(entry => entry.SupportingSpecies).ThenInclude(species => species.Care)
            .Include(entry => entry.SupportingSpecies).ThenInclude(species => species.Toxicity)
            .Include(entry => entry.SupportingSpecies).ThenInclude(species => species.Flowering)
            .FirstOrDefaultAsync(entry => entry.Id == organismId);

        return organism is not null
            ? Ok(_mapper.Map<IEnumerable<PlantSpeciesDto>>(
                organism.SupportingSpecies.OrderBy(species => species.Name)))
            : NotFound();
    }

    /// <summary>Replaces the whole set of collection species that support this organism.</summary>
    [RequiresPermission(Permissions.AlmanacPropose)]
    [HttpPut("{organismId:guid}/species")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async ValueTask<IActionResult> ReplaceSpecies([FromRoute] Guid organismId,
        [FromBody] IReadOnlyCollection<Guid> speciesIds)
    {
        BeneficialOrganism? organism = await _dbContext.BeneficialOrganisms
            .Include(entry => entry.SupportingSpecies)
            .FirstOrDefaultAsync(entry => entry.Id == organismId);

        if (organism is null) return NotFound();

        if (!await ModelState.RequireAllExistAsync<PlantSpecies>(_dbContext, speciesIds, "speciesIds"))
            return UnprocessableEntity(new ValidationProblemDetails(ModelState));

        List<PlantSpecies> species = await _dbContext.PlantSpecies
            .Where(entry => speciesIds.Contains(entry.Id))
            .ToListAsync();

        organism.SupportingSpecies.Clear();
        organism.SupportingSpecies.AddRange(species);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("{organismId:guid}/pests")]
    [ProducesResponseType<IEnumerable<PestDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async ValueTask<ActionResult<IEnumerable<PestDto>>> ListPests([FromRoute] Guid organismId)
    {
        BeneficialOrganism? organism = await _dbContext.BeneficialOrganisms
            .AsNoTracking()
            .Include(entry => entry.PestsControlled)
            .FirstOrDefaultAsync(entry => entry.Id == organismId);

        return organism is not null
            ? Ok(_mapper.Map<IEnumerable<PestDto>>(organism.PestsControlled.OrderBy(pest => pest.Name)))
            : NotFound();
    }

    /// <summary>Replaces the whole set of pests this organism preys on.</summary>
    [RequiresPermission(Permissions.AlmanacPropose)]
    [HttpPut("{organismId:guid}/pests")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async ValueTask<IActionResult> ReplacePests([FromRoute] Guid organismId,
        [FromBody] IReadOnlyCollection<Guid> pestIds)
    {
        BeneficialOrganism? organism = await _dbContext.BeneficialOrganisms
            .Include(entry => entry.PestsControlled)
            .FirstOrDefaultAsync(entry => entry.Id == organismId);

        if (organism is null) return NotFound();

        if (!await ModelState.RequireAllExistAsync<Pest>(_dbContext, pestIds, "pestIds"))
            return UnprocessableEntity(new ValidationProblemDetails(ModelState));

        List<Pest> pests = await _dbContext.Pests
            .Where(pest => pestIds.Contains(pest.Id))
            .ToListAsync();

        organism.PestsControlled.Clear();
        organism.PestsControlled.AddRange(pests);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }
}
