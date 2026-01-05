namespace PlantKeeperAPI.Entities;

public class TreatmentLog
{
    public Guid Id { get; set; }
    public Guid PlantId { get; set; }
    public Guid TreatmentId { get; set; }
    public DateTime Date { get; set; }
    public string? Comments { get; set; }

    public Plant Plant { get; init; } = null!;
    public Treatment Treatment { get; init; } = null!;
}
