using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace PlantKeeperAPI.Models;

[DebuggerDisplay("Watering method: {Name}")]
public class InputWateringMethod
{
    [StringLength(20)]
    public required string Name { get; set; }

    [StringLength(255)]
    public string? Description { get; set; }
}
