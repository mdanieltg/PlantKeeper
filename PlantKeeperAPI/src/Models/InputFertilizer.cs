using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using PlantKeeperAPI.Enums;

namespace PlantKeeperAPI.Models;

[DebuggerDisplay("Fertilizer: {Name}")]
public class InputFertilizer
{
    [StringLength(30)]
    public required string Name { get; set; }

    [StringLength(255)]
    public string? Description { get; set; }

    /// <summary>Label ratio such as "21-0-0" or "17-17-17".</summary>
    [StringLength(15)]
    public string? NpkRatio { get; set; }

    public FertilizerCategory Category { get; set; }
}
