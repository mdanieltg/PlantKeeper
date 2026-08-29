namespace PlantKeeperAPI.Enums;

/// <summary>
/// Whether a species flowers under cultivation. Pairs with
/// <c>PlantSpecies.Flowering</c>: <see cref="DoesNotFlower" /> means no flowering
/// profile row exists, which keeps "never flowers" distinct from "not researched yet".
/// </summary>
public enum FloweringHabit
{
    FlowersInCultivation,

    /// <summary>Flowers in nature but seldom or never in a pot, as with ivy and jade.</summary>
    RarelyFlowersInCultivation,

    DoesNotFlower
}
