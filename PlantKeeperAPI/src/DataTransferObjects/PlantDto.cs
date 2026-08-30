using System.Diagnostics;

namespace PlantKeeperAPI.DataTransferObjects;

[DebuggerDisplay("Plant: {Alias}")]
public class PlantDto
{
    public Guid Id { get; set; }
    public required string Alias { get; set; }
    public Guid SpeciesId { get; set; }
    public string? Comments { get; set; }
}
