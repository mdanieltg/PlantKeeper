namespace PlantKeeperAPI.Enums;

/// <summary>
/// Ordered light scale. Used as a range (<c>LightMin</c>..<c>LightMax</c>) because the
/// almanac states light as a span, for example "sombra parcial a sol".
/// </summary>
public enum LightLevel
{
    LowLight,
    Shade,
    PartialShade,
    BrightIndirect,
    FullSun
}
