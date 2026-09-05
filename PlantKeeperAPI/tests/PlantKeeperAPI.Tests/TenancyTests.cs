using System.Net;
using System.Net.Http.Json;
using AwesomeAssertions;

namespace PlantKeeperAPI.Tests;

/// <summary>
/// What one keeper can reach of another's collection. The answer is meant to be "nothing,
/// and indistinguishably from it not existing".
/// </summary>
[TestFixture]
public class TenancyTests
{
    private static KeeperSession A => TenancyFixture.A;
    private static KeeperSession B => TenancyFixture.B;

    [Test]
    public async Task ListingPlants_ShowsOnlyTheSignedInKeepersOwn()
    {
        PlantSummary[] seenByA = await A.ReadAsync<PlantSummary[]>("/api/plants");
        PlantSummary[] seenByB = await B.ReadAsync<PlantSummary[]>("/api/plants");

        seenByA.Select(plant => plant.Alias).Should().ContainSingle().Which.Should().Be("Monstera de A");
        seenByB.Select(plant => plant.Alias).Should().ContainSingle().Which.Should().Be("Monstera de B");
    }

    [Test]
    public async Task ListingLogs_ShowsOnlyTheSignedInKeepersOwn()
    {
        LogSummary[] seenByA = await A.ReadAsync<LogSummary[]>("/api/observation-logs");
        LogSummary[] seenByB = await B.ReadAsync<LogSummary[]>("/api/observation-logs");

        seenByA.Should().ContainSingle().Which.Id.Should().Be(TenancyFixture.LogOfA);
        seenByB.Should().ContainSingle().Which.Id.Should().Be(TenancyFixture.LogOfB);
    }

    /// <summary>
    /// The one the plan called out. A leak here is silent - a 200 with someone else's plant
    /// in it looks exactly like a working endpoint.
    /// </summary>
    [Test]
    public async Task GettingAnotherKeepersPlantById_IsNotFound()
    {
        HttpResponseMessage response = await A.GetAsync($"/api/plants/{TenancyFixture.PlantOfB}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound,
            "another keeper's plant must be indistinguishable from one that does not exist");
    }

    [Test]
    public async Task GettingAnotherKeepersLogById_IsNotFound()
    {
        HttpResponseMessage response = await A.GetAsync($"/api/observation-logs/{TenancyFixture.LogOfB}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Uses a plant of B's created for this test rather than the shared fixture one. A write
    /// that is <em>supposed</em> to be refused still lands if the protection regresses, and
    /// a destructive test working on shared rows would then hand every later test a 404 for
    /// the wrong reason. Found by removing the query filter and watching which tests stayed
    /// green.
    /// </summary>
    [Test]
    public async Task UpdatingAnotherKeepersPlant_IsNotFound()
    {
        Guid plantId = await B.CreateAsync("/api/plants",
            new { alias = "Intocable", speciesId = TenancyFixture.SpeciesId });

        HttpResponseMessage response = await A.PutAsync($"/api/plants/{plantId}",
            new { alias = "hijacked", speciesId = TenancyFixture.SpeciesId });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        PlantSummary plant = await B.ReadAsync<PlantSummary>($"/api/plants/{plantId}");
        plant.Alias.Should().Be("Intocable", "the update must not have gone through");

        await B.DeleteAsync($"/api/plants/{plantId}");
    }

    /// <summary>Its own plant, for the reason given on the update test above.</summary>
    [Test]
    public async Task DeletingAnotherKeepersPlant_IsNotFound()
    {
        Guid plantId = await B.CreateAsync("/api/plants",
            new { alias = "Tampoco se borra", speciesId = TenancyFixture.SpeciesId });

        HttpResponseMessage response = await A.DeleteAsync($"/api/plants/{plantId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        HttpResponseMessage stillThere = await B.GetAsync($"/api/plants/{plantId}");
        stillThere.StatusCode.Should().Be(HttpStatusCode.OK, "the delete must not have gone through");

        await B.DeleteAsync($"/api/plants/{plantId}");
    }

    /// <summary>
    /// Writes are the other direction of the same leak: not reading someone else's row, but
    /// attaching one of your own to it.
    /// </summary>
    [Test]
    public async Task LoggingAgainstAnotherKeepersPlant_IsRejected()
    {
        HttpResponseMessage response = await A.PostAsync("/api/observation-logs",
            new { plantId = TenancyFixture.PlantOfB, date = "2026-09-02T09:00:00Z", notes = "injected" });

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);

        ValidationProblem? problem =
            await response.Content.ReadFromJsonAsync<ValidationProblem>(KeeperSession.Json);
        problem!.Errors.Should().ContainKey("plantId");
    }

    [Test]
    public async Task SourcingAPropagationBatchFromAnotherKeepersPlant_IsRejected()
    {
        HttpResponseMessage response = await A.PostAsync("/api/propagation-batches", new
        {
            speciesId = TenancyFixture.SpeciesId,
            sourcePlantId = TenancyFixture.PlantOfB,
            startDate = "2026-09-01T00:00:00Z",
            count = 3,
            medium = "Water",
            rootingHormone = "Optional"
        });

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);

        ValidationProblem? problem =
            await response.Content.ReadFromJsonAsync<ValidationProblem>(KeeperSession.Json);
        problem!.Errors.Should().ContainKey("sourcePlantId");
    }

    /// <summary>A query-string filter is not a way around the row filter.</summary>
    [Test]
    public async Task FilteringLogsByAnotherKeepersPlantId_ReturnsNothing()
    {
        LogSummary[] logs =
            await A.ReadAsync<LogSummary[]>($"/api/observation-logs?plantId={TenancyFixture.PlantOfB}");

        logs.Should().BeEmpty();
    }

    /// <summary>
    /// Filters govern reads, not writes. A created row that took no owner would be invisible
    /// to everyone including the keeper who made it.
    /// </summary>
    [Test]
    public async Task ACreatedPlant_IsVisibleToItsOwnerAndNobodyElse()
    {
        Guid plantId = await A.CreateAsync("/api/plants",
            new { alias = "Recién plantada", speciesId = TenancyFixture.SpeciesId });

        HttpResponseMessage owner = await A.GetAsync($"/api/plants/{plantId}");
        HttpResponseMessage stranger = await B.GetAsync($"/api/plants/{plantId}");

        owner.StatusCode.Should().Be(HttpStatusCode.OK, "ownership must be stamped on write");
        stranger.StatusCode.Should().Be(HttpStatusCode.NotFound);

        await A.DeleteAsync($"/api/plants/{plantId}");
    }

    /// <summary>Ownership is not something a request body gets to state.</summary>
    [Test]
    public async Task ACreatedPlant_IgnoresAKeeperIdInTheRequestBody()
    {
        Guid plantId = await A.CreateAsync("/api/plants", new
        {
            alias = "Con dueño ajeno",
            speciesId = TenancyFixture.SpeciesId,
            keeperId = B.KeeperId
        });

        HttpResponseMessage stranger = await B.GetAsync($"/api/plants/{plantId}");
        stranger.StatusCode.Should().Be(HttpStatusCode.NotFound,
            "the body must not be able to hand a row to another keeper");

        HttpResponseMessage owner = await A.GetAsync($"/api/plants/{plantId}");
        owner.StatusCode.Should().Be(HttpStatusCode.OK);

        await A.DeleteAsync($"/api/plants/{plantId}");
    }

    /// <summary>The almanac is shared on purpose - the filters must not have reached it.</summary>
    [Test]
    public async Task TheAlmanacIsVisibleToBothKeepers()
    {
        HttpResponseMessage seenByA = await A.GetAsync($"/api/plant-species/{TenancyFixture.SpeciesId}");
        HttpResponseMessage seenByB = await B.GetAsync($"/api/plant-species/{TenancyFixture.SpeciesId}");

        seenByA.StatusCode.Should().Be(HttpStatusCode.OK);
        seenByB.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private sealed record PlantSummary(Guid Id, string Alias);

    private sealed record LogSummary(Guid Id, Guid PlantId, string Notes);

    private sealed record ValidationProblem(Dictionary<string, string[]> Errors);
}
