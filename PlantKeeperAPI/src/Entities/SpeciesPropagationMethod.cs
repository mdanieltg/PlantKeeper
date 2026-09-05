using System.Diagnostics;
using PlantKeeperAPI.Enums;

namespace PlantKeeperAPI.Entities;

/// <summary>
/// One row of the almanac propagation matrix. Rooting hormone, season and difficulty
/// hang off this link rather than off the species: the almanac often qualifies them per
/// method, as with Retama, whose hormone advice applies to the cutting and not to the
/// primary seed method.
/// </summary>
[DebuggerDisplay("Propagation link (primary: {IsPrimary})")]
public class SpeciesPropagationMethod : IAlmanacVersioned
{
    public Guid Id { get; set; }

    /// <summary>Bumped on every saved change. See <see cref="IAlmanacVersioned" />.</summary>
    public int Version { get; set; }
    public Guid SpeciesId { get; set; }
    public Guid PropagationMethodId { get; set; }
    public bool IsPrimary { get; set; }
    public RootingHormoneUse RootingHormone { get; set; }
    public string? BestSeason { get; set; }
    public PropagationDifficulty? Difficulty { get; set; }
    public string? Notes { get; set; }

    public PlantSpecies Species { get; init; } = null!;
    public PropagationMethod Method { get; init; } = null!;
}
