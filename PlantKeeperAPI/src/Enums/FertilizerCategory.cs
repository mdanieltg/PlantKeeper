namespace PlantKeeperAPI.Enums;

/// <summary>Column groups of the almanac fertilization matrix.</summary>
public enum FertilizerCategory
{
    /// <summary>Ammonium sulfate (21-0-0) and other nitrogen-only feeds.</summary>
    NitrogenOnly,

    /// <summary>Balanced NPK such as Triple 10, 15 or 17.</summary>
    Balanced,

    /// <summary>Low nitrogen, high phosphorus-potassium bloom feeds.</summary>
    Bloom,

    /// <summary>Slow-release organics: compost, worm humus, guano.</summary>
    Organic
}
