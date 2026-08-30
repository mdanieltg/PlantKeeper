using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace PlantKeeperAPI.Models;

[DebuggerDisplay("Treatment: {Name}")]
public class InputTreatment
{
    [StringLength(30)]
    public required string Name { get; set; }

    [StringLength(255)]
    public string? Description { get; set; }
}
