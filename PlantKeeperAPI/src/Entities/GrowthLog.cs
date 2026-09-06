namespace PlantKeeperAPI.Entities;

public class GrowthLog : IKeeperOwned
{
    public Guid Id { get; set; }

    /// <summary>
    /// Owning keeper. Denormalized here rather than reached through the plant: a global
    /// query filter on only one end of a required relationship makes EF warn, and the join
    /// costs more than the column.
    /// </summary>
    public Guid KeeperId { get; set; }
    public Guid PlantId { get; set; }
    public DateTimeOffset Date { get; set; }
    public decimal? HeightCm { get; set; }
    public decimal? HeightToLastNodeCm { get; set; }
    public string? Notes { get; set; }

    public Plant Plant { get; init; } = null!;
}
