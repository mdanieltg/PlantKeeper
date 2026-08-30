using System.ComponentModel.DataAnnotations;

namespace PlantKeeperAPI.Models;

public class InputTreatmentLog
{
    public Guid PlantId { get; set; }
    public Guid TreatmentId { get; set; }
    public DateTime Date { get; set; }

    [StringLength(255)]
    public string? Comments { get; set; }
}
