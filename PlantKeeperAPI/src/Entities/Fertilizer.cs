using System.Diagnostics;
using PlantKeeperAPI.Enums;

namespace PlantKeeperAPI.Entities;

[DebuggerDisplay("Fertilization: {Name}")]
public class Fertilizer
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }

    /// <summary>Label ratio such as "21-0-0" or "17-17-17".</summary>
    public string? NpkRatio { get; set; }

    /// <summary>Maps this product onto a column of the almanac fertilization matrix.</summary>
    public FertilizerCategory Category { get; set; }

    public List<FertilizationLog> Logs { get; } = [];
}
