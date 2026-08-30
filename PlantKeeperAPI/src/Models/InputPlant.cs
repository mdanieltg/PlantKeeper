using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace PlantKeeperAPI.Models;

[DebuggerDisplay("Plant: {Alias}")]
public class InputPlant
{
    [StringLength(50)]
    public required string Alias { get; set; }

    public Guid SpeciesId { get; set; }

    [StringLength(255)]
    public string? Comments { get; set; }
}
