using System.ComponentModel.DataAnnotations;
using PlantKeeperAPI.Enums;

namespace PlantKeeperAPI.Models;

/// <summary>Required for every species - an unresearched one must never read as harmless.</summary>
public class InputSpeciesToxicityProfile
{
    public Toxicity ToHumans { get; set; }

    [StringLength(150)]
    public string? ToHumansNotes { get; set; }

    public Toxicity ToPets { get; set; }

    [StringLength(150)]
    public string? ToPetsNotes { get; set; }
}
