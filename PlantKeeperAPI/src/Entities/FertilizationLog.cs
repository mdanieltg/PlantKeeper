namespace PlantKeeperAPI.Entities;

public class FertilizationLog
{
    public Guid Id { get; set; }
    public Guid PlantId { get; set; }
    public Guid FertilizationMethodId { get; set; }
    public DateTime Date { get; set; }
    public string? Comments { get; set; }

    public Plant Plant { get; init; } = null!;
    public FertilizationMethod FertilizationMethod { get; init; } = null!;
}
