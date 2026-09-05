using PlantKeeperAPI.Enums;

namespace PlantKeeperAPI.Entities;

/// <summary>A batch of propagules currently rooting.</summary>
public class PropagationBatch : IKeeperOwned
{
    public Guid Id { get; set; }

    /// <summary>Owning keeper. The global query filter compares against this.</summary>
    public Guid KeeperId { get; set; }
    public Guid SpeciesId { get; set; }
    public Guid? SourcePlantId { get; set; }
    public Guid? PropagationMethodId { get; set; }
    public DateTime StartDate { get; set; }
    public int Count { get; set; }
    public PropagationMedium Medium { get; set; }
    public RootingHormoneUse RootingHormone { get; set; }
    public string? Status { get; set; }
    public string? TargetTransplantWindow { get; set; }
    public string? Notes { get; set; }

    public PlantSpecies Species { get; init; } = null!;
    public Plant? SourcePlant { get; init; }
    public PropagationMethod? Method { get; init; }
}
