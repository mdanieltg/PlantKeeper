namespace PlantKeeperAPI.Entities;

public class ObservationLog
{
    public Guid Id { get; set; }
    public Guid PlantId { get; set; }
    public DateTime ObservationDate { get; set; }
    public required string Notes { get; set; }

    public Plant Plant { get; init; } = null!;
}
