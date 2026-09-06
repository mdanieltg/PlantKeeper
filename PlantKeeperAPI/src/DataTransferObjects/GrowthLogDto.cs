namespace PlantKeeperAPI.DataTransferObjects;

public class GrowthLogDto
{
    public Guid Id { get; set; }
    public Guid PlantId { get; set; }
    public DateTimeOffset Date { get; set; }
    public decimal? HeightCm { get; set; }
    public decimal? HeightToLastNodeCm { get; set; }
    public string? Notes { get; set; }
}
