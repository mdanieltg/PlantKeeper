using System.ComponentModel.DataAnnotations;

namespace PlantKeeperAPI.Models;

public class InputObservationLog
{
    public Guid PlantId { get; set; }
    public DateTimeOffset Date { get; set; }

    [StringLength(300)]
    public required string Notes { get; set; }
}
