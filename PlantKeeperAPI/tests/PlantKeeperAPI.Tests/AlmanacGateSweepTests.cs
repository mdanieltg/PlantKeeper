using System.Net;
using AwesomeAssertions;

namespace PlantKeeperAPI.Tests;

/// <summary>
/// The gate has to be on <em>every</em> almanac write, not the ones that were easy to
/// remember. One endpoint left ungated is a way to edit the shared almanac without review,
/// and nothing else in the suite would notice.
/// <para>
/// Deletes carry no body, so they sweep cheaply: a missed gate shows up as 204 with the row
/// gone, rather than as a validation error that would pass vacuously.
/// </para>
/// </summary>
[TestFixture]
public class AlmanacGateSweepTests
{
    private static KeeperSession A => TenancyFixture.A;
    private static KeeperSession B => TenancyFixture.B;

    private readonly List<string> _created = [];

    private static IEnumerable<TestCaseData> DeletableAlmanacRows()
    {
        yield return Row("/api/climates", new
        {
            name = "Sweep clima", temperature = "18-26", precipitation = "media",
            humidity = "60%", sun = "parcial", wind = "suave"
        });
        yield return Row("/api/potting-mixes",
            new { name = "Sweep mezcla", composition = "turba", drainage = "buena" });
        yield return Row("/api/watering-methods",
            new { name = "Sweep riego", description = "riego de prueba" });
        yield return Row("/api/fertilizers",
            new { name = "Sweep abono", composition = "orgánico", application = "mensual" });
        yield return Row("/api/treatments",
            new { name = "Sweep tratamiento", type = "fungicida", application = "foliar" });
        yield return Row("/api/propagation-methods",
            new { name = "Sweep propagación", description = "esqueje" });
        yield return Row("/api/pests",
            new { name = "Sweep plaga", type = "insecto", damage = "hojas", signs = "manchas" });
        yield return Row("/api/beneficial-organisms", new
        {
            name = "Sweep organismo", role = "Pollinator",
            description = "poliniza", howToAttract = "flores"
        });

        static TestCaseData Row(string collection, object body) =>
            new TestCaseData(collection, body).SetArgDisplayNames(collection);
    }

    [TearDown]
    public async Task CleanUpAsync()
    {
        foreach (string path in _created) await A.DeleteAsync(path);

        _created.Clear();
    }

    [Test]
    [TestCaseSource(nameof(DeletableAlmanacRows))]
    public async Task ANonApproverCannotDeleteAnAlmanacRow(string collection, object body)
    {
        Guid id = await A.CreateAsync(collection, body);
        _created.Insert(0, $"{collection}/{id}");

        HttpResponseMessage response = await B.DeleteAsync($"{collection}/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.Accepted,
            $"a delete on {collection} must queue for review, not go through");

        (await A.GetAsync($"{collection}/{id}")).StatusCode
            .Should().Be(HttpStatusCode.OK, "the row must still be there");
    }

    [Test]
    public async Task ANonApproverCannotDeleteASpecies()
    {
        Guid climateId = await A.CreateAsync("/api/climates", new
        {
            name = "Sweep clima especie", temperature = "18-26", precipitation = "media",
            humidity = "60%", sun = "parcial", wind = "suave"
        });
        _created.Add($"/api/climates/{climateId}");

        Guid mixId = await A.CreateAsync("/api/potting-mixes",
            new { name = "Sweep mezcla especie", composition = "turba", drainage = "buena" });

        Guid speciesId = await A.CreateAsync("/api/plant-species", new
        {
            scientificName = "Sweep species", name = "Sweep",
            climateId, pottingMixId = mixId,
            floweringHabit = "DoesNotFlower", fertilizationFrequency = "mensual",
            care = new
            {
                lightMin = "PartialShade", lightMax = "BrightIndirect",
                minTemperatureCelsius = 12, maxTemperatureCelsius = 30,
                wateringRequirement = "riego moderado", soilPhMin = 5.5m, soilPhMax = 7.0m,
                windTolerance = "Low"
            },
            toxicity = new { toHumans = "Irritant", toPets = "MildlyToxic" }
        });

        _created.Insert(0, $"/api/plant-species/{speciesId}");
        _created.Insert(1, $"/api/potting-mixes/{mixId}");

        (await B.DeleteAsync($"/api/plant-species/{speciesId}")).StatusCode
            .Should().Be(HttpStatusCode.Accepted);
        (await A.GetAsync($"/api/plant-species/{speciesId}")).StatusCode
            .Should().Be(HttpStatusCode.OK);

        // The nested writes are gated too - a species' care profile is almanac content.
        HttpResponseMessage care = await B.PutAsync($"/api/plant-species/{speciesId}/care", new
        {
            lightMin = "Shade", lightMax = "FullSun",
            minTemperatureCelsius = 5, maxTemperatureCelsius = 35,
            wateringRequirement = "cambiado", soilPhMin = 6.0m, soilPhMax = 7.5m,
            windTolerance = "High"
        });

        care.StatusCode.Should().Be(HttpStatusCode.Accepted, "the profiles are almanac content too");

        // The matrices as well.
        HttpResponseMessage matrix = await B.PostAsync(
            $"/api/plant-species/{speciesId}/fertilizer-recommendations",
            new { category = "Organic", suitability = "Recommended", notes = "prueba" });

        matrix.StatusCode.Should().Be(HttpStatusCode.Accepted, "the matrices are almanac content too");
    }

    [Test]
    public async Task ANonApproverCannotReplaceAPestsTreatments()
    {
        Guid pestId = await A.CreateAsync("/api/pests",
            new { name = "Sweep plaga enlaces", type = "ácaro", damage = "hojas", signs = "telaraña" });
        _created.Insert(0, $"/api/pests/{pestId}");

        Guid treatmentId = await A.CreateAsync("/api/treatments",
            new { name = "Sweep tratamiento enlaces", type = "acaricida", application = "foliar" });
        _created.Insert(1, $"/api/treatments/{treatmentId}");

        HttpResponseMessage response = await B.PutAsync($"/api/pests/{pestId}/treatments",
            new[] { treatmentId });

        response.StatusCode.Should().Be(HttpStatusCode.Accepted,
            "replacing a set is an almanac edit like any other");

        Treatment[] linked = await A.ReadAsync<Treatment[]>($"/api/pests/{pestId}/treatments");
        linked.Should().BeEmpty("the set must be unchanged until review");
    }

    private sealed record Treatment(Guid Id, string Name);
}
