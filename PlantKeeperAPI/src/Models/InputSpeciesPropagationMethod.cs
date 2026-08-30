using System.ComponentModel.DataAnnotations;
using PlantKeeperAPI.Enums;

namespace PlantKeeperAPI.Models;

/// <summary>One row of the propagation matrix. The species comes from the route.</summary>
public class InputSpeciesPropagationMethod
{
    public Guid PropagationMethodId { get; set; }
    public bool IsPrimary { get; set; }
    public RootingHormoneUse RootingHormone { get; set; }

    [StringLength(50)]
    public string? BestSeason { get; set; }

    public PropagationDifficulty? Difficulty { get; set; }

    [StringLength(300)]
    public string? Notes { get; set; }
}
