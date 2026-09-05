namespace PlantKeeperAPI.Entities;

/// <summary>
/// An entity that belongs to one keeper's collection rather than to the shared almanac.
/// <para>
/// Implementing this is what puts a row behind a global query filter, so the set of types
/// carrying it is the tenancy boundary. Eight today: <see cref="Plant" />,
/// <see cref="PropagationBatch" /> and the six log types.
/// </para>
/// </summary>
public interface IKeeperOwned
{
    Guid KeeperId { get; set; }
}
