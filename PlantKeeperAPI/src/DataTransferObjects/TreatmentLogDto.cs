namespace PlantKeeperAPI.DataTransferObjects;

public class TreatmentLogDto
{
    public Guid Id { get; set; }
    public Guid PlantId { get; set; }
    public Guid TreatmentId { get; set; }
    public DateTime Date { get; set; }
    public string? Comments { get; set; }
}
