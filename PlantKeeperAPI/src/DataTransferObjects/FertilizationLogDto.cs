namespace PlantKeeperAPI.DataTransferObjects;

public class FertilizationLogDto
{
    public Guid Id { get; set; }
    public Guid PlantId { get; set; }
    public Guid FertilizerId { get; set; }
    public DateTimeOffset Date { get; set; }
    public string? Dose { get; set; }
    public string? Comments { get; set; }
}
