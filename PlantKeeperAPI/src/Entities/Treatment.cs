using System.Diagnostics;

namespace PlantKeeperAPI.Entities;

[DebuggerDisplay("Treatment: {Name}")]
public class Treatment : IAlmanacVersioned
{
    public Guid Id { get; set; }

    /// <summary>Bumped on every saved change. See <see cref="IAlmanacVersioned" />.</summary>
    public int Version { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }

    public List<TreatmentLog> Logs { get; } = [];
    public List<Pest> Pests { get; } = [];
    public List<SpeciesTreatmentRecommendation> SpeciesRecommendations { get; } = [];
}
