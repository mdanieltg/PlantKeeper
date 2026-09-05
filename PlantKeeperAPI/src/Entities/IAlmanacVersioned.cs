namespace PlantKeeperAPI.Entities;

/// <summary>
/// An almanac row that carries a version, so a change proposed against it can be rejected
/// if the row moved on before a moderator got to it.
/// <para>
/// The version is bumped centrally in <c>SaveChanges</c>, never by a caller. A proposal
/// records the version it was written against; approval compares that against the row as it
/// stands now. Fifteen types implement this - the whole shared almanac.
/// </para>
/// </summary>
public interface IAlmanacVersioned
{
    /// <summary>Starts at 1 and increases by one on every saved change.</summary>
    int Version { get; set; }
}
