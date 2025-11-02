using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace PlantKeeperAPI.Models;

[DebuggerDisplay("Plant: {Name}")]
public class InputPlant
{
    [StringLength(30)]
    public required string Name { get; set; }

    [StringLength(255)]
    public string? Care { get; set; }
}
