using System.Net;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PlantKeeperAPI.Database;

namespace PlantKeeperAPI.Tests;

/// <summary>
/// What a delete takes with it. The rule is where the row sits: inside one keeper's
/// aggregate it cascades, across the shared almanac it is refused.
/// <para>
/// Every test here builds and destroys its own rows. The shared fixture is read-only, and
/// these are the tests most able to damage it.
/// </para>
/// </summary>
[TestFixture]
public class DeleteBehaviourTests
{
    private static KeeperSession A => TenancyFixture.A;

    /// <summary>Everything this test made, newest first.</summary>
    private readonly List<string> _created = [];

    /// <summary>
    /// Removes what the test created, most dependent first - which is reverse creation
    /// order, now that Restrict means a climate cannot go before the species using it.
    /// <para>
    /// Not optional housekeeping. The tenancy tests assert that a keeper sees exactly their
    /// own rows, so a plant left behind here fails a test three files away - which is how
    /// this teardown came to exist.
    /// </para>
    /// </summary>
    [TearDown]
    public async Task CleanUpAsync()
    {
        foreach (string path in _created) await A.DeleteAsync(path);

        _created.Clear();
    }

    private async Task<Guid> TrackAsync(string collection, object body)
    {
        Guid id = await A.CreateAsync(collection, body);
        _created.Insert(0, $"{collection}/{id}");
        return id;
    }

    private async Task<Guid> CreateClimateAsync(string name) =>
        await TrackAsync("/api/climates", new
        {
            name, temperature = "18-26", precipitation = "media",
            humidity = "60%", sun = "parcial", wind = "suave"
        });

    private async Task<Guid> CreateSpeciesAsync(Guid climateId, string scientificName)
    {
        Guid pottingMixId = await TrackAsync("/api/potting-mixes",
            new { name = $"Mezcla {scientificName}", composition = "turba, perlita", drainage = "buena" });

        return await TrackAsync("/api/plant-species", new
        {
            scientificName,
            name = scientificName,
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
    }

    // ---- refused across the almanac boundary --------------------------------------

    [Test]
    public async Task DeletingAClimateASpeciesUses_IsConflict()
    {
        Guid climateId = await CreateClimateAsync("Clima en uso");
        await CreateSpeciesAsync(climateId, "Ficus lyrata");

        HttpResponseMessage response = await A.DeleteAsync($"/api/climates/{climateId}");

        response.StatusCode.Should().Be(HttpStatusCode.Conflict,
            "a cascade here would destroy species belonging to keepers the deleter cannot see");

        Conflict? conflict = await response.Content.ReadFromJsonAsync<Conflict>(KeeperSession.Json);
        conflict!.Title.Should().Be("Still in use");
        conflict.Detail.Should().Contain("plant species");
        conflict.ReferencedBy.Should().Be("PlantSpecies");

        // And the climate really is still there.
        (await A.GetAsync($"/api/climates/{climateId}")).StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Test]
    public async Task DeletingAnUnusedClimate_Succeeds()
    {
        Guid climateId = await CreateClimateAsync("Clima sin usar");

        HttpResponseMessage response = await A.DeleteAsync($"/api/climates/{climateId}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent, "restrict must not block an unused row");
        (await A.GetAsync($"/api/climates/{climateId}")).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task DeletingASpeciesAPlantUses_IsConflict()
    {
        Guid climateId = await CreateClimateAsync("Clima para especie con planta");
        Guid speciesId = await CreateSpeciesAsync(climateId, "Sansevieria trifasciata");
        Guid plantId = await TrackAsync("/api/plants", new { alias = "Lengua de suegra", speciesId });

        HttpResponseMessage response = await A.DeleteAsync($"/api/plant-species/{speciesId}");

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        Conflict? conflict = await response.Content.ReadFromJsonAsync<Conflict>(KeeperSession.Json);
        conflict!.ReferencedBy.Should().Be("Plants");

        await A.DeleteAsync($"/api/plants/{plantId}");
        (await A.DeleteAsync($"/api/plant-species/{speciesId}")).StatusCode
            .Should().Be(HttpStatusCode.NoContent, "the species goes once nothing points at it");
    }

    [Test]
    public async Task DeletingAWateringMethodALogUses_IsConflict()
    {
        Guid climateId = await CreateClimateAsync("Clima para riego");
        Guid speciesId = await CreateSpeciesAsync(climateId, "Epipremnum aureum");
        Guid plantId = await TrackAsync("/api/plants", new { alias = "Potus", speciesId });

        Guid methodId = await TrackAsync("/api/watering-methods",
            new { name = "Inmersión", description = "sumergir la maceta" });

        await A.CreateAsync("/api/watering-logs",
            new { plantId, wateringMethodId = methodId, date = "2026-09-01T09:00:00Z" });

        HttpResponseMessage response = await A.DeleteAsync($"/api/watering-methods/{methodId}");

        response.StatusCode.Should().Be(HttpStatusCode.Conflict,
            "the lookup is shared, so its logs may belong to keepers the deleter cannot see");

        Conflict? conflict = await response.Content.ReadFromJsonAsync<Conflict>(KeeperSession.Json);
        conflict!.ReferencedBy.Should().Be("WateringLogs");
    }

    // ---- cascades inside one keeper's own tree ------------------------------------

    /// <summary>
    /// The other half of the rule. Restricting here would make a plant undeletable until
    /// its owner cleared six log types by hand.
    /// </summary>
    [Test]
    public async Task DeletingAPlant_TakesAllSixLogTypesWithIt()
    {
        Guid climateId = await CreateClimateAsync("Clima para cascada");
        Guid speciesId = await CreateSpeciesAsync(climateId, "Chlorophytum comosum");
        Guid plantId = await TrackAsync("/api/plants", new { alias = "Cinta", speciesId });

        Guid wateringMethodId = await TrackAsync("/api/watering-methods",
            new { name = "Regadera fina", description = "riego superficial" });
        Guid fertilizerId = await TrackAsync("/api/fertilizers",
            new { name = "Humus líquido", composition = "orgánico", application = "quincenal" });
        Guid treatmentId = await TrackAsync("/api/treatments",
            new { name = "Jabón potásico", type = "insecticida", application = "foliar" });

        await A.CreateAsync("/api/watering-logs",
            new { plantId, wateringMethodId, date = "2026-09-01T09:00:00Z" });
        await A.CreateAsync("/api/fertilization-logs",
            new { plantId, fertilizerId, date = "2026-09-01T09:00:00Z", dose = "5 ml" });
        await A.CreateAsync("/api/treatment-logs",
            new { plantId, treatmentId, date = "2026-09-01T09:00:00Z" });
        await A.CreateAsync("/api/repotting-logs",
            new { plantId, date = "2026-09-01T09:00:00Z", dimensions = "15cm", volume = "2L", material = "barro" });
        await A.CreateAsync("/api/observation-logs",
            new { plantId, date = "2026-09-01T09:00:00Z", notes = "hijuelos" });
        await A.CreateAsync("/api/growth-logs",
            new { plantId, date = "2026-09-01T09:00:00Z", heightCm = 22.5m });

        (await CountLogsAsync(plantId)).Should().Be(6, "the fixture must actually have six logs to lose");

        HttpResponseMessage response = await A.DeleteAsync($"/api/plants/{plantId}");
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        (await CountLogsAsync(plantId)).Should().Be(0, "a plant's logs belong to it and go with it");
    }

    /// <summary>
    /// A keeper's whole tree goes with them, and none of the Restrict rules gets in the way.
    /// <para>
    /// The plan's phrase for this is "nothing orphaned, nothing blocked". Both halves matter:
    /// a Restrict pointed the wrong way would make an account undeletable, and a missing
    /// cascade would leave rows behind that no query filter will ever match again.
    /// </para>
    /// </summary>
    [Test]
    public async Task DeletingAKeeper_RemovesTheirWholeTreeAndTheirIdentityRows()
    {
        Guid climateId = await CreateClimateAsync("Clima para llavero");
        Guid speciesId = await CreateSpeciesAsync(climateId, "Zamioculcas zamiifolia");

        string userName = $"keeper-{Guid.NewGuid():N}"[..20];
        Guid keeperId = await TestKeepers.CreateAsync(TenancyFixture.Factory, userName);

        using (KeeperSession session = await KeeperSession.SignInAsync(TenancyFixture.Factory, userName))
        {
            Guid plantId = await session.CreateAsync("/api/plants", new { alias = "Zamioculca", speciesId });
            await session.CreateAsync("/api/observation-logs",
                new { plantId, date = "2026-09-01T09:00:00Z", notes = "brote" });
            await session.CreateAsync("/api/growth-logs",
                new { plantId, date = "2026-09-01T09:00:00Z", heightCm = 30.0m });
        }

        (await CountOwnedRowsAsync(keeperId)).Should().Be(3, "one plant and two logs to lose");

        await TestKeepers.DeleteAsync(TenancyFixture.Factory, keeperId);

        (await CountOwnedRowsAsync(keeperId)).Should().Be(0, "nothing orphaned");
        (await CountIdentityRowsAsync(keeperId)).Should().Be(0, "their roles go too");

        // The species they used is untouched - it belongs to the shared almanac.
        (await A.GetAsync($"/api/plant-species/{speciesId}")).StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private static async Task<int> CountOwnedRowsAsync(Guid keeperId)
    {
        await using AsyncServiceScope scope = TenancyFixture.Factory.Services.CreateAsyncScope();
        PlantKeeperDbContext dbContext = scope.ServiceProvider.GetRequiredService<PlantKeeperDbContext>();

        return await dbContext.Plants.IgnoreQueryFilters().CountAsync(row => row.KeeperId == keeperId)
               + await dbContext.PropagationBatches.IgnoreQueryFilters().CountAsync(row => row.KeeperId == keeperId)
               + await dbContext.WateringLogs.IgnoreQueryFilters().CountAsync(row => row.KeeperId == keeperId)
               + await dbContext.FertilizationLogs.IgnoreQueryFilters().CountAsync(row => row.KeeperId == keeperId)
               + await dbContext.TreatmentLogs.IgnoreQueryFilters().CountAsync(row => row.KeeperId == keeperId)
               + await dbContext.RepottingLogs.IgnoreQueryFilters().CountAsync(row => row.KeeperId == keeperId)
               + await dbContext.ObservationLogs.IgnoreQueryFilters().CountAsync(row => row.KeeperId == keeperId)
               + await dbContext.GrowthLogs.IgnoreQueryFilters().CountAsync(row => row.KeeperId == keeperId);
    }

    private static async Task<int> CountIdentityRowsAsync(Guid keeperId)
    {
        await using AsyncServiceScope scope = TenancyFixture.Factory.Services.CreateAsyncScope();
        PlantKeeperDbContext dbContext = scope.ServiceProvider.GetRequiredService<PlantKeeperDbContext>();

        return await dbContext.UserRoles.CountAsync(row => row.UserId == keeperId)
               + await dbContext.UserClaims.CountAsync(row => row.UserId == keeperId)
               + await dbContext.Users.CountAsync(row => row.Id == keeperId);
    }

    /// <summary>
    /// Counts straight from the database, past the query filters. Asking the API would only
    /// prove the rows are invisible, which is what a leaked orphan looks like too.
    /// </summary>
    private static async Task<int> CountLogsAsync(Guid plantId)
    {
        await using AsyncServiceScope scope = TenancyFixture.Factory.Services.CreateAsyncScope();
        PlantKeeperDbContext dbContext = scope.ServiceProvider.GetRequiredService<PlantKeeperDbContext>();

        return await dbContext.WateringLogs.IgnoreQueryFilters().CountAsync(log => log.PlantId == plantId)
               + await dbContext.FertilizationLogs.IgnoreQueryFilters().CountAsync(log => log.PlantId == plantId)
               + await dbContext.TreatmentLogs.IgnoreQueryFilters().CountAsync(log => log.PlantId == plantId)
               + await dbContext.RepottingLogs.IgnoreQueryFilters().CountAsync(log => log.PlantId == plantId)
               + await dbContext.ObservationLogs.IgnoreQueryFilters().CountAsync(log => log.PlantId == plantId)
               + await dbContext.GrowthLogs.IgnoreQueryFilters().CountAsync(log => log.PlantId == plantId);
    }

    private sealed record Conflict(string Title, string Detail, string? ReferencedBy);
}
