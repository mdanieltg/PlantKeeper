namespace PlantKeeperAPI.Entities;

public class WateringLog
{
    public Guid Id { get; set; }
    public Guid PlantId { get; set; }
    public Guid WateringMethodId { get; set; }
    public Guid KeeperId { get; set; }
    public DateTime Date { get; set; }
    public string? Comments { get; set; }

    public Plant Plant { get; init; } = null!;
    public WateringMethod WateringMethod { get; init; } = null!;
}
