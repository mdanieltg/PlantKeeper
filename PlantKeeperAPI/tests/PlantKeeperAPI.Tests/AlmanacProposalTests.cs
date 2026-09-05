using System.Net;
using AwesomeAssertions;

namespace PlantKeeperAPI.Tests;

/// <summary>
/// Change control on the shared almanac. Keeper A holds <c>almanac.approve</c> through the
/// moderator role; keeper B holds only <c>almanac.propose</c>.
/// </summary>
[TestFixture]
public class AlmanacProposalTests
{
    private static KeeperSession A => TenancyFixture.A;
    private static KeeperSession B => TenancyFixture.B;

    private readonly List<string> _created = [];

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

    private Task<Guid> CreateClimateAsync(string name) => TrackAsync("/api/climates", new
    {
        name, temperature = "18-26", precipitation = "media",
        humidity = "60%", sun = "parcial", wind = "suave"
    });

    private static object ClimateBody(string name) => new
    {
        name, temperature = "18-26", precipitation = "media",
        humidity = "60%", sun = "parcial", wind = "suave"
    };

    // ---- the approver's own edits ----------------------------------------------------

    [Test]
    public async Task AnApproversEdit_AppliesImmediatelyAndIsRecordedAutoApproved()
    {
        Guid climateId = await CreateClimateAsync("Clima original");

        HttpResponseMessage response = await A.PutAsync($"/api/climates/{climateId}",
            ClimateBody("Clima corregido"));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent, "an approver never waits");

        Climate climate = await A.ReadAsync<Climate>($"/api/climates/{climateId}");
        climate.Name.Should().Be("Clima corregido");

        Proposal[] proposals = await A.ReadAsync<Proposal[]>("/api/almanac-proposals");
        Proposal edit = proposals.First(p => p.TargetId == climateId && p.Operation == "Update");

        edit.Status.Should().Be("Applied");
        edit.AutoApproved.Should().BeTrue("the workflow still runs - it just costs an approver nothing");
        edit.TargetVersion.Should().Be(1, "the version it was written against");
    }

    /// <summary>A create references nothing yet, so it needs no review from anyone.</summary>
    [Test]
    public async Task ACreateByANonApprover_AppliesImmediately()
    {
        HttpResponseMessage response = await B.PostAsync("/api/climates", ClimateBody("Clima de B"));

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        Climate created = (await response.Content.ReadFromJsonAsync<Climate>(KeeperSession.Json))!;
        _created.Insert(0, $"/api/climates/{created.Id}");

        Proposal[] proposals = await A.ReadAsync<Proposal[]>("/api/almanac-proposals");
        proposals.Should().Contain(p => p.TargetType == "Climate"
                                        && p.Operation == "Create"
                                        && p.Status == "Applied"
                                        && p.AutoApproved);
    }

    // ---- the queue -------------------------------------------------------------------

    [Test]
    public async Task ANonApproversEdit_QueuesAndLeavesTheAlmanacAlone()
    {
        Guid climateId = await CreateClimateAsync("Clima intacto");

        HttpResponseMessage response = await B.PutAsync($"/api/climates/{climateId}",
            ClimateBody("Clima que B quiere"));

        response.StatusCode.Should().Be(HttpStatusCode.Accepted);

        Proposal queued = (await response.Content.ReadFromJsonAsync<Proposal>(KeeperSession.Json))!;
        queued.Status.Should().Be("Pending");
        queued.AutoApproved.Should().BeFalse();
        queued.ProposedById.Should().Be(B.KeeperId);

        Climate climate = await A.ReadAsync<Climate>($"/api/climates/{climateId}");
        climate.Name.Should().Be("Clima intacto", "nothing may change until a moderator says so");

        // ... and approving it is what finally applies it.
        HttpResponseMessage approval = await A.PostAsync(
            $"/api/almanac-proposals/{queued.Id}/approve", new { note = "correcto" });

        approval.StatusCode.Should().Be(HttpStatusCode.OK);

        climate = await A.ReadAsync<Climate>($"/api/climates/{climateId}");
        climate.Name.Should().Be("Clima que B quiere");
    }

    [Test]
    public async Task ARejectedProposal_NeverTouchesTheAlmanac()
    {
        Guid climateId = await CreateClimateAsync("Clima que sobrevive");

        Proposal queued = await QueueEditAsync(climateId, "Clima rechazado");

        HttpResponseMessage rejection = await A.PostAsync(
            $"/api/almanac-proposals/{queued.Id}/reject", new { note = "el nombre ya existe" });

        rejection.StatusCode.Should().Be(HttpStatusCode.OK);

        Proposal decided = await A.ReadAsync<Proposal>($"/api/almanac-proposals/{queued.Id}");
        decided.Status.Should().Be("Rejected");
        decided.ReviewNote.Should().Be("el nombre ya existe");

        Climate climate = await A.ReadAsync<Climate>($"/api/climates/{climateId}");
        climate.Name.Should().Be("Clima que sobrevive");
    }

    [Test]
    public async Task ADecisionIsMadeOnce()
    {
        Guid climateId = await CreateClimateAsync("Clima decidido");
        Proposal queued = await QueueEditAsync(climateId, "Primer intento");

        (await A.PostAsync($"/api/almanac-proposals/{queued.Id}/approve", new { }))
            .StatusCode.Should().Be(HttpStatusCode.OK);

        HttpResponseMessage again =
            await A.PostAsync($"/api/almanac-proposals/{queued.Id}/approve", new { });

        again.StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await again.Content.ReadFromJsonAsync<ReviewProblem>(KeeperSession.Json))!
            .Reason.Should().Be("AlreadyDecided");
    }

    // ---- staleness -------------------------------------------------------------------

    /// <summary>
    /// The reason a proposal records the version it was written against. Approving over a
    /// row that moved on would silently undo whatever moved it.
    /// </summary>
    [Test]
    public async Task AProposalAgainstASinceChangedTarget_IsRefusedNotApplied()
    {
        Guid climateId = await CreateClimateAsync("Clima v1");

        Proposal queued = await QueueEditAsync(climateId, "Lo que B escribió");

        // A moderator edits the same row while the proposal waits.
        (await A.PutAsync($"/api/climates/{climateId}", ClimateBody("Lo que A cambió")))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);

        HttpResponseMessage approval =
            await A.PostAsync($"/api/almanac-proposals/{queued.Id}/approve", new { });

        approval.StatusCode.Should().Be(HttpStatusCode.Conflict);

        ReviewProblem problem =
            (await approval.Content.ReadFromJsonAsync<ReviewProblem>(KeeperSession.Json))!;
        problem.Reason.Should().Be("Stale");
        problem.Detail.Should().Contain("has changed since this was written");

        Climate climate = await A.ReadAsync<Climate>($"/api/climates/{climateId}");
        climate.Name.Should().Be("Lo que A cambió", "the stale body must not have been written over it");

        Proposal decided = await A.ReadAsync<Proposal>($"/api/almanac-proposals/{queued.Id}");
        decided.Status.Should().Be("Rejected", "a refusal is a decision, not a retry");
    }

    [Test]
    public async Task AProposalAgainstADeletedTarget_IsRefused()
    {
        Guid climateId = await CreateClimateAsync("Clima efímero");
        Proposal queued = await QueueEditAsync(climateId, "Ya no existe");

        (await A.DeleteAsync($"/api/climates/{climateId}")).StatusCode.Should().Be(HttpStatusCode.NoContent);
        _created.Remove($"/api/climates/{climateId}");

        HttpResponseMessage approval =
            await A.PostAsync($"/api/almanac-proposals/{queued.Id}/approve", new { });

        approval.StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await approval.Content.ReadFromJsonAsync<ReviewProblem>(KeeperSession.Json))!
            .Reason.Should().Be("TargetGone");
    }

    // ---- authorization ---------------------------------------------------------------

    [Test]
    public async Task ANonApprover_CannotApproveTheirOwnProposal()
    {
        Guid climateId = await CreateClimateAsync("Clima sin auto-aprobar");
        Proposal queued = await QueueEditAsync(climateId, "B se aprueba a sí mismo");

        HttpResponseMessage response =
            await B.PostAsync($"/api/almanac-proposals/{queued.Id}/approve", new { });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden,
            "the queue would be theatre if a proposer could clear it");

        Climate climate = await A.ReadAsync<Climate>($"/api/climates/{climateId}");
        climate.Name.Should().Be("Clima sin auto-aprobar");
    }

    private async Task<Proposal> QueueEditAsync(Guid climateId, string newName)
    {
        HttpResponseMessage response = await B.PutAsync($"/api/climates/{climateId}", ClimateBody(newName));
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);

        return (await response.Content.ReadFromJsonAsync<Proposal>(KeeperSession.Json))!;
    }

    private sealed record Climate(Guid Id, string Name);

    private sealed record Proposal(
        Guid Id, string TargetType, Guid? TargetId, string Operation, int? TargetVersion,
        string Status, bool AutoApproved, Guid ProposedById, string? ReviewNote);

    private sealed record ReviewProblem(string Title, string Detail, string Reason);
}
