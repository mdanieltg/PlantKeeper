namespace PlantKeeperAPI.Entities;

public class FertilizationLog
{
    public Guid Id { get; set; }
    public Guid PlantId { get; set; }
    public Guid FertilizerId { get; set; }
    public DateTime Date { get; set; }
    public string? Dose { get; set; }
    public string? Comments { get; set; }

    public Plant Plant { get; init; } = null!;
    public Fertilizer FertilizerUsed { get; init; } = null!;
}
