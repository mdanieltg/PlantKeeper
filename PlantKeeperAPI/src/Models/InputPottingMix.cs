using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace PlantKeeperAPI.Models;

[DebuggerDisplay("Potting mix: {Name}")]
public class InputPottingMix
{
    [StringLength(30)]
    public required string Name { get; set; }

    [StringLength(255)]
    public string? Description { get; set; }
}
