using PlantKeeperAPI.Enums;

namespace PlantKeeperAPI.DataTransferObjects;

public class PropagationBatchDto
{
    public Guid Id { get; set; }
    public Guid SpeciesId { get; set; }
    public Guid? SourcePlantId { get; set; }
    public Guid? PropagationMethodId { get; set; }
    public DateTimeOffset StartDate { get; set; }
    public int Count { get; set; }
    public PropagationMedium Medium { get; set; }
    public RootingHormoneUse RootingHormone { get; set; }
    public string? Status { get; set; }
    public string? TargetTransplantWindow { get; set; }
    public string? Notes { get; set; }
}
