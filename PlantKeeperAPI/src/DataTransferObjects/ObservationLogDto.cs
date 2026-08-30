namespace PlantKeeperAPI.DataTransferObjects;

public class ObservationLogDto
{
    public Guid Id { get; set; }
    public Guid PlantId { get; set; }
    public DateTime Date { get; set; }
    public required string Notes { get; set; }
}
