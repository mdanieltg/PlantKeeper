namespace PlantKeeperAPI.Entities;

public class RepottingLog
{
    public Guid Id { get; set; }
    public Guid PlantId { get; set; }
    public DateTime Date { get; set; }
    public string? Comments { get; set; }

    public Plant Plant { get; init; } = null!;
}
