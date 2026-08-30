using System.Diagnostics;
using PlantKeeperAPI.Enums;

namespace PlantKeeperAPI.DataTransferObjects;

[DebuggerDisplay("Propagation link (primary: {IsPrimary})")]
public class SpeciesPropagationMethodDto
{
    public Guid Id { get; set; }
    public Guid SpeciesId { get; set; }
    public Guid PropagationMethodId { get; set; }
    public bool IsPrimary { get; set; }
    public RootingHormoneUse RootingHormone { get; set; }
    public string? BestSeason { get; set; }
    public PropagationDifficulty? Difficulty { get; set; }
    public string? Notes { get; set; }
}
