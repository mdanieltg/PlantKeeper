using System.ComponentModel.DataAnnotations;

namespace PlantKeeperAPI.Models;

public class InputFertilizationLog
{
    public Guid PlantId { get; set; }
    public Guid FertilizerId { get; set; }
    public DateTimeOffset Date { get; set; }

    [StringLength(100)]
    public string? Dose { get; set; }

    [StringLength(255)]
    public string? Comments { get; set; }
}
