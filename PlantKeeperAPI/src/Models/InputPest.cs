using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using PlantKeeperAPI.Enums;

namespace PlantKeeperAPI.Models;

[DebuggerDisplay("Pest: {Name}")]
public class InputPest
{
    [StringLength(50)]
    public required string Name { get; set; }

    [StringLength(255)]
    public string? Description { get; set; }

    public PestStatus Status { get; set; }

    [StringLength(300)]
    public string? Notes { get; set; }
}
