using System.ComponentModel.DataAnnotations;
using PlantKeeperAPI.Enums;

namespace PlantKeeperAPI.Models;

/// <summary>One cell of the fertilization matrix. The species comes from the route.</summary>
public class InputSpeciesFertilizerRecommendation
{
    public FertilizerCategory Category { get; set; }
    public FertilizerSuitability Suitability { get; set; }

    [StringLength(255)]
    public string? Notes { get; set; }
}
