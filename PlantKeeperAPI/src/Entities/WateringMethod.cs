using System.Diagnostics;

namespace PlantKeeperAPI.Entities;

[DebuggerDisplay("Watering method: {Name}")]
public class WateringMethod : IAlmanacVersioned
{
    public Guid Id { get; set; }

    /// <summary>Bumped on every saved change. See <see cref="IAlmanacVersioned" />.</summary>
    public int Version { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }

    public List<WateringLog> Logs { get; } = [];
}
