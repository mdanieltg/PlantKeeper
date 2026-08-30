using System.ComponentModel.DataAnnotations;
using PlantKeeperAPI.Enums;

namespace PlantKeeperAPI.Models;

public class InputPropagationBatch
{
    public Guid SpeciesId { get; set; }
    public Guid? SourcePlantId { get; set; }
    public Guid? PropagationMethodId { get; set; }
    public DateTime StartDate { get; set; }

    [Range(0, int.MaxValue)]
    public int Count { get; set; }

    public PropagationMedium Medium { get; set; }
    public RootingHormoneUse RootingHormone { get; set; }

    [StringLength(255)]
    public string? Status { get; set; }

    [StringLength(50)]
    public string? TargetTransplantWindow { get; set; }

    [StringLength(300)]
    public string? Notes { get; set; }
}
