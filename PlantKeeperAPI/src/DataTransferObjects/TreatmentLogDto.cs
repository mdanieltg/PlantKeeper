namespace PlantKeeperAPI.DataTransferObjects;

public class TreatmentLogDto
{
    public Guid Id { get; set; }
    public Guid PlantId { get; set; }
    public Guid TreatmentId { get; set; }
    public DateTimeOffset Date { get; set; }
    public string? Comments { get; set; }
}
