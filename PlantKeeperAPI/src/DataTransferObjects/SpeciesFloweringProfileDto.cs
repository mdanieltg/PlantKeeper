using System.Diagnostics;

namespace PlantKeeperAPI.DataTransferObjects;

[DebuggerDisplay("Flowering: {BloomSeason}")]
public class SpeciesFloweringProfileDto
{
    public Guid SpeciesId { get; set; }
    public required string BloomSeason { get; set; }
    public required string BloomCareNotes { get; set; }
    public required string SeedViability { get; set; }
    public required string SeedHarvestTiming { get; set; }
}
