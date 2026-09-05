namespace PlantKeeperAPI.Entities;

public class TreatmentLog : IKeeperOwned
{
    public Guid Id { get; set; }

    /// <summary>
    /// Owning keeper. Denormalized here rather than reached through the plant: a global
    /// query filter on only one end of a required relationship makes EF warn, and the join
    /// costs more than the column.
    /// </summary>
    public Guid KeeperId { get; set; }
    public Guid PlantId { get; set; }
    public Guid TreatmentId { get; set; }
    public DateTime Date { get; set; }
    public string? Comments { get; set; }

    public Plant Plant { get; init; } = null!;
    public Treatment Treatment { get; init; } = null!;
}
