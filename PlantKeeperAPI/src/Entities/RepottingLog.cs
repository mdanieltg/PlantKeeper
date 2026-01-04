namespace PlantKeeperAPI.Entities;

public class RepottingLog
{
    public Guid Id { get; set; }
    public Guid PlantId { get; set; }
    public DateTime Date { get; set; }
    public required string Dimensions { get; set; }
    public required string Volume { get; set; }
    public required string Material { get; set; }
    public string? Comments { get; set; }

    public Plant Plant { get; init; } = null!;
}
