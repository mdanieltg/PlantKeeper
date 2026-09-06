using System.ComponentModel.DataAnnotations;

namespace PlantKeeperAPI.Models;

public class InputGrowthLog
{
    public Guid PlantId { get; set; }
    public DateTimeOffset Date { get; set; }

    [Range(0, 99999.9)]
    public decimal? HeightCm { get; set; }

    [Range(0, 99999.9)]
    public decimal? HeightToLastNodeCm { get; set; }

    [StringLength(300)]
    public string? Notes { get; set; }
}
