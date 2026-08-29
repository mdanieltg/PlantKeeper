namespace PlantKeeperAPI.Enums;

/// <summary>
/// Roles a beneficial organism can play. Combinable: hoverflies both pollinate as
/// adults and predate aphids as larvae.
/// </summary>
[Flags]
public enum BeneficialRole
{
    Pollinator = 1,
    Predator = 2,
    Parasitoid = 4
}
