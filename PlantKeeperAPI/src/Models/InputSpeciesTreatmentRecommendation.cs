using System.ComponentModel.DataAnnotations;
using PlantKeeperAPI.Enums;

namespace PlantKeeperAPI.Models;

/// <summary>One cell of the pest-control matrix. The species comes from the route.</summary>
public class InputSpeciesTreatmentRecommendation
{
    public Guid TreatmentId { get; set; }
    public TreatmentSafety Safety { get; set; }

    [StringLength(255)]
    public string? Notes { get; set; }
}
