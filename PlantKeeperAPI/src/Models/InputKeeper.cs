using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace PlantKeeperAPI.Models;

[DebuggerDisplay("Keeper: {Name}")]
public class InputKeeper
{
    [StringLength(20)]
    public required string Name { get; set; }
}
