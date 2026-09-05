using System.Net;
using AwesomeAssertions;

namespace PlantKeeperAPI.Tests;

/// <summary>
/// The default-deny pipeline. These share the tenancy fixture because they need the same
/// running API, not because they are about tenancy.
/// </summary>
[TestFixture]
public class AuthorizationTests
{
    private static IEnumerable<string> ProtectedPaths =>
    [
        "/api/plants", "/api/climates", "/api/plant-species", "/api/observation-logs",
        "/api/pests", "/api/propagation-batches", "/api/authentication/me"
    ];

    [Test]
    [TestCaseSource(nameof(ProtectedPaths))]
    public async Task AnUnauthenticatedRequest_IsUnauthorized(string path)
    {
        using HttpClient anonymous = TenancyFixture.Factory.CreateClient();

        HttpResponseMessage response = await anonymous.GetAsync(path);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
            "the fallback policy covers every endpoint that does not opt out");
    }

    /// <summary>
    /// Keeper B holds the keeper role only, so almanac.approve, keepers.manage and
    /// roles.manage are all missing. Nothing is gated on those yet - what this pins is the
    /// distinction that matters when something is: a missing permission is 403, not 401.
    /// </summary>
    [Test]
    public async Task KeeperB_HoldsOnlyTheKeeperRolesPermissions()
    {
        KeeperSession.SignedInKeeper me =
            await TenancyFixture.B.ReadAsync<KeeperSession.SignedInKeeper>("/api/authentication/me");

        me.Roles.Should().BeEquivalentTo("keeper");
        me.Permissions.Should().BeEquivalentTo(
            "almanac.propose", "almanac.read", "plants.read", "plants.write");
        me.Permissions.Should().NotContain("almanac.approve");
    }

    [Test]
    public async Task TheBootstrapEndpoint_IsClosedWhenNoSecretIsConfigured()
    {
        using HttpClient anonymous = TenancyFixture.Factory.CreateClient();

        HttpResponseMessage response = await anonymous.PostAsJsonAsync(
            "/api/authentication/bootstrap-password",
            new { userName = TestKeepers.A, secret = "anything", password = "IrrelevantPassword1!" });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound,
            "with no Bootstrap:Secret the endpoint must not exist at all");
    }

    /// <summary>
    /// Signing out rotates the security stamp, so a captured cookie stops working rather
    /// than merely being forgotten by the browser.
    /// <para>
    /// Uses its own keeper. Stamp rotation revokes <em>every</em> ticket that keeper holds,
    /// so running this against a shared account signs the rest of the fixture out mid-run -
    /// which is how this test found the behaviour it now asserts.
    /// </para>
    /// </summary>
    [Test]
    public async Task SigningOut_RevokesTheTicketRatherThanJustClearingIt()
    {
        using KeeperSession session =
            await KeeperSession.SignInAsync(TenancyFixture.Factory, TestKeepers.Disposable);

        (await session.GetAsync("/api/plants")).StatusCode.Should().Be(HttpStatusCode.OK);

        HttpResponseMessage signOut = await session.PostAsync("/api/authentication/sign-out", new { });
        signOut.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Same cookie, replayed.
        HttpResponseMessage replayed = await session.GetAsync("/api/plants");
        replayed.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
