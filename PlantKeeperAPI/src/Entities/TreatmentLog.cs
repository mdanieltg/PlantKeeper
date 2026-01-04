namespace PlantKeeperAPI.Entities;

public class TreatmentLog
{
    public Guid Id { get; set; }
    public Guid PlantId { get; set; }
    public Guid TreatmentMethodId { get; set; }
    public DateTime Date { get; set; }
    public string? Comments { get; set; }

    public Plant Plant { get; init; } = null!;
    public TreatmentMethod TreatmentMethod { get; init; } = null!;
}
