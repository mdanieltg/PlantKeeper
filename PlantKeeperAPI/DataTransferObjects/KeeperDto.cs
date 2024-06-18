using System.Diagnostics;

namespace PlantKeeperAPI.DataTransferObjects;

[DebuggerDisplay("Keeper: {Name}")]
public class KeeperDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
}
