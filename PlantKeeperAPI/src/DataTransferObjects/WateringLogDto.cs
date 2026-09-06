namespace PlantKeeperAPI.DataTransferObjects;

public class WateringLogDto
{
    public Guid Id { get; set; }
    public Guid PlantId { get; set; }
    public Guid WateringMethodId { get; set; }
    public DateTimeOffset Date { get; set; }
    public string? Comments { get; set; }
}
