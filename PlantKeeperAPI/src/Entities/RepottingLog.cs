namespace PlantKeeperAPI.Entities;

public class RepottingLog : IKeeperOwned
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
    public required string Dimensions { get; set; }
    public required string Volume { get; set; }
    public required string Material { get; set; }
    public string? Comments { get; set; }

    public Plant Plant { get; init; } = null!;
}
