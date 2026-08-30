using System.ComponentModel.DataAnnotations;

namespace PlantKeeperAPI.Models;

/// <summary>
/// Optional as a whole, which is why every member is required: a species that does not
/// flower has no profile at all, so a profile that exists is complete.
/// </summary>
public class InputSpeciesFloweringProfile
{
    [StringLength(150)]
    public required string BloomSeason { get; set; }

    [StringLength(255)]
    public required string BloomCareNotes { get; set; }

    [StringLength(255)]
    public required string SeedViability { get; set; }

    [StringLength(150)]
    public required string SeedHarvestTiming { get; set; }
}
