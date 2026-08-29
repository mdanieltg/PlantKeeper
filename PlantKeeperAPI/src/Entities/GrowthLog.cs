namespace PlantKeeperAPI.Entities;

public class GrowthLog
{
    public Guid Id { get; set; }
    public Guid PlantId { get; set; }
    public DateTime Date { get; set; }
    public decimal? HeightCm { get; set; }
    public decimal? HeightToLastNodeCm { get; set; }
    public string? Notes { get; set; }

    public Plant Plant { get; init; } = null!;
}
