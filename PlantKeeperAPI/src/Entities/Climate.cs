using System.Diagnostics;

namespace PlantKeeperAPI.Entities;

[DebuggerDisplay("Climate: {Name}")]
public class Climate
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Temperature { get; set; }
    public required string Precipitation { get; set; }
    public required string Humidity { get; set; }
    public required string Sun { get; set; }
    public required string Wind { get; set; }
    public string? Description { get; set; }

    public List<PlantSpecies> SpeciesList { get; } = [];
}
