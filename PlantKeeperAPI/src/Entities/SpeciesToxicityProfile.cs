using System.Diagnostics;
using PlantKeeperAPI.Enums;

namespace PlantKeeperAPI.Entities;

/// <summary>
/// Toxicity to people and pets. Required on purpose: an unresearched species must never
/// read as harmless.
/// </summary>
[DebuggerDisplay("Toxicity: humans {ToHumans}, pets {ToPets}")]
public class SpeciesToxicityProfile : IAlmanacVersioned
{
    /// <summary>Primary key and foreign key both — one profile per species.</summary>
    public Guid SpeciesId { get; set; }

    /// <summary>Bumped on every saved change. See <see cref="IAlmanacVersioned" />.</summary>
    public int Version { get; set; }

    public Toxicity ToHumans { get; set; }
    public string? ToHumansNotes { get; set; }

    public Toxicity ToPets { get; set; }
    public string? ToPetsNotes { get; set; }

    public PlantSpecies Species { get; init; } = null!;
}
