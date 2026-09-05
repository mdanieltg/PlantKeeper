namespace PlantKeeperAPI.Tests;

/// <summary>
/// The world the tenancy tests observe: a shared almanac, and two keepers who each own one
/// plant and one observation log.
/// <para>
/// Built once for the assembly. These tests are about what one keeper can see of another,
/// which needs both collections to exist at the same time - so the fixture is shared and
/// the tests read it rather than each building a world of their own.
/// </para>
/// </summary>
[SetUpFixture]
public class TenancyFixture
{
    public static PlantKeeperApiFactory Factory { get; private set; } = null!;

    public static KeeperSession A { get; private set; } = null!;
    public static KeeperSession B { get; private set; } = null!;

    /// <summary>Shared almanac rows. Both keepers can see these; that is the point of the almanac.</summary>
    public static Guid SpeciesId { get; private set; }

    public static Guid PlantOfA { get; private set; }
    public static Guid PlantOfB { get; private set; }

    public static Guid LogOfA { get; private set; }
    public static Guid LogOfB { get; private set; }

    [OneTimeSetUp]
    public async Task StartAsync()
    {
        // Before the host, not after: SeedFirstKeeperAsync skips with a warning when
        // migrations are pending, so an unmigrated database would start an API with no
        // keeper in it and every test would fail on sign-in instead of on its assertion.
        TestDatabase.Reset();

        Factory = new PlantKeeperApiFactory();
        await TestKeepers.EnsureAsync(Factory);

        A = await KeeperSession.SignInAsync(Factory, TestKeepers.A);
        B = await KeeperSession.SignInAsync(Factory, TestKeepers.B);

        Guid climateId = await A.CreateAsync("/api/climates", new
        {
            name = "Templado", temperature = "18-26", precipitation = "media",
            humidity = "60%", sun = "parcial", wind = "suave"
        });

        Guid pottingMixId = await A.CreateAsync("/api/potting-mixes", new
        {
            name = "Universal", composition = "turba, perlita, humus", drainage = "buena"
        });

        SpeciesId = await A.CreateAsync("/api/plant-species", new
        {
            scientificName = "Monstera deliciosa",
            name = "Costilla de Adán",
            climateId,
            pottingMixId,
            floweringHabit = "DoesNotFlower",
            fertilizationFrequency = "mensual",
            care = new
            {
                lightMin = "PartialShade", lightMax = "BrightIndirect",
                minTemperatureCelsius = 12, maxTemperatureCelsius = 30,
                wateringRequirement = "riego moderado", soilPhMin = 5.5m, soilPhMax = 7.0m,
                windTolerance = "Low"
            },
            toxicity = new { toHumans = "Irritant", toPets = "MildlyToxic" }
        });

        PlantOfA = await A.CreateAsync("/api/plants", new { alias = "Monstera de A", speciesId = SpeciesId });
        PlantOfB = await B.CreateAsync("/api/plants", new { alias = "Monstera de B", speciesId = SpeciesId });

        LogOfA = await A.CreateAsync("/api/observation-logs",
            new { plantId = PlantOfA, date = "2026-09-01T09:00:00Z", notes = "hoja nueva" });
        LogOfB = await B.CreateAsync("/api/observation-logs",
            new { plantId = PlantOfB, date = "2026-09-01T09:00:00Z", notes = "raíz aérea" });
    }

    [OneTimeTearDown]
    public void Stop()
    {
        A.Dispose();
        B.Dispose();
        Factory.Dispose();
    }
}
