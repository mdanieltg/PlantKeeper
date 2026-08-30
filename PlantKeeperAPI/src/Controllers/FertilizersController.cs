using System.Net.Mime;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using PlantKeeperAPI.Database;
using PlantKeeperAPI.DataTransferObjects;
using PlantKeeperAPI.Entities;
using PlantKeeperAPI.Models;

namespace PlantKeeperAPI.Controllers;

[ApiController]
[Route("/api/fertilizers")]
[Consumes(MediaTypeNames.Application.Json)]
[Produces(MediaTypeNames.Application.Json)]
public class FertilizersController : ControllerBase
{
    private readonly PlantKeeperDbContext _dbContext;
    private readonly IMapper _mapper;

    public FertilizersController(PlantKeeperDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    [HttpGet]
    [ProducesResponseType<IEnumerable<FertilizerDto>>(StatusCodes.Status200OK)]
    public IEnumerable<FertilizerDto> List() => _mapper.Map<IEnumerable<FertilizerDto>>(
        _dbContext.Fertilizers.OrderBy(fertilizer => fertilizer.Name)
    );

    [HttpPost]
    [ProducesResponseType<FertilizerDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async ValueTask<ActionResult<FertilizerDto>> Create([FromBody] InputFertilizer fertilizer)
    {
        var fertilizerToCreate = _mapper.Map<Fertilizer>(fertilizer);
        await _dbContext.Fertilizers.AddAsync(fertilizerToCreate);
        await _dbContext.SaveChangesAsync();

        var fertilizerToReturn = _mapper.Map<FertilizerDto>(fertilizerToCreate);
        return CreatedAtAction(nameof(Get), new { fertilizerId = fertilizerToReturn.Id }, fertilizerToReturn);
    }

    [HttpGet("{fertilizerId:guid}")]
    [ProducesResponseType<FertilizerDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async ValueTask<ActionResult<FertilizerDto>> Get([FromRoute] Guid fertilizerId)
    {
        Fertilizer? fertilizer = await _dbContext.Fertilizers.FindAsync(fertilizerId);
        return fertilizer is not null
            ? Ok(_mapper.Map<FertilizerDto>(fertilizer))
            : NotFound();
    }

    [HttpPut("{fertilizerId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async ValueTask<IActionResult> Update([FromRoute] Guid fertilizerId, [FromBody] InputFertilizer fertilizer)
    {
        Fertilizer? currentFertilizer = await _dbContext.Fertilizers.FindAsync(fertilizerId);
        if (currentFertilizer is null) return NotFound();

        _mapper.Map(fertilizer, currentFertilizer);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{fertilizerId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async ValueTask<IActionResult> Delete([FromRoute] Guid fertilizerId)
    {
        Fertilizer? fertilizer = await _dbContext.Fertilizers.FindAsync(fertilizerId);
        if (fertilizer is null) return NotFound();

        _dbContext.Remove(fertilizer);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }
}
