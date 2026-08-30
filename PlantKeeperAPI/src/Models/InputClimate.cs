using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace PlantKeeperAPI.Models;

[DebuggerDisplay("Climate: {Name}")]
public class InputClimate
{
    [StringLength(50)]
    public required string Name { get; set; }

    [StringLength(50)]
    public required string Temperature { get; set; }

    [StringLength(50)]
    public required string Precipitation { get; set; }

    [StringLength(50)]
    public required string Humidity { get; set; }

    [StringLength(50)]
    public required string Sun { get; set; }

    [StringLength(50)]
    public required string Wind { get; set; }

    [StringLength(255)]
    public string? Description { get; set; }
}
