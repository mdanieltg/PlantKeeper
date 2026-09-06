using System.ComponentModel.DataAnnotations;

namespace PlantKeeperAPI.Models;

public class InputRepottingLog
{
    public Guid PlantId { get; set; }
    public DateTimeOffset Date { get; set; }

    [StringLength(50)]
    public required string Dimensions { get; set; }

    [StringLength(30)]
    public required string Volume { get; set; }

    [StringLength(50)]
    public required string Material { get; set; }

    [StringLength(255)]
    public string? Comments { get; set; }
}
