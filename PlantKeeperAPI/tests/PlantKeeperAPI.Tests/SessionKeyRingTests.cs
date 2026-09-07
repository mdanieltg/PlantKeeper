using System.Net;
using AwesomeAssertions;

namespace PlantKeeperAPI.Tests;

/// <summary>
/// What survives a redeploy. The session cookie is a ticket encrypted with the
/// DataProtection key ring, so where that ring is kept decides whether restarting the API
/// signs everybody out.
/// <para>
/// This is the failure mode a one-host test suite cannot see: every assertion passes, the
/// app works perfectly, and the only people who notice are the ones who were already signed
/// in when it was deployed. So these two boot a second host on purpose - one over the same
/// key directory, one over an empty one - which is exactly the difference a mounted volume
/// makes.
/// </para>
/// </summary>
[TestFixture]
public class SessionKeyRingTests
{
    private string _keyRing = null!;
    private string _emptyKeyRing = null!;

    [SetUp]
    public void CreateKeyDirectories()
    {
        _keyRing = Directory.CreateTempSubdirectory("plantkeeper-keys-").FullName;
        _emptyKeyRing = Directory.CreateTempSubdirectory("plantkeeper-keys-").FullName;
    }

    [TearDown]
    public void RemoveKeyDirectories()
    {
        Directory.Delete(_keyRing, recursive: true);
        Directory.Delete(_emptyKeyRing, recursive: true);
    }

    [Test]
    public async Task A_session_survives_the_api_restarting_when_the_key_ring_persists()
    {
        string cookie = await SignInThenStopAsync(_keyRing);

        using HttpResponseMessage replayed = await ReplayAsync(cookie, _keyRing);

        replayed.StatusCode.Should().Be(HttpStatusCode.OK,
            "the second host read the same keys, so the ticket issued by the first still decrypts");
    }

    [Test]
    public async Task A_session_is_lost_when_the_key_ring_is_not()
    {
        string cookie = await SignInThenStopAsync(_keyRing);

        using HttpResponseMessage replayed = await ReplayAsync(cookie, _emptyKeyRing);

        replayed.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
            "a container without the volume starts with an empty ring, which is the whole reason for one");
    }

    /// <summary>
    /// Signs in against a host keeping its keys in <paramref name="keyRing" />, then stops
    /// it - the redeploy. Returns the cookie that host issued.
    /// </summary>
    private static async Task<string> SignInThenStopAsync(string keyRing)
    {
        await using PlantKeeperApiFactory factory = new(keyRing);
        using HttpClient client = factory.CreateClient();

        using HttpResponseMessage response = await client.PostAsJsonAsync("/api/authentication/sign-in",
            new { userName = TestKeepers.A, password = TestKeepers.Password, rememberMe = false });

        response.EnsureSuccessStatusCode();

        return response.Headers.GetValues("Set-Cookie")
            .Select(header => header.Split(';')[0])
            .Single(value => value.StartsWith("plantkeeper.session=", StringComparison.Ordinal));
    }

    /// <summary>Starts a second host over <paramref name="keyRing" /> and replays the cookie at it.</summary>
    private static async Task<HttpResponseMessage> ReplayAsync(string cookie, string keyRing)
    {
        await using PlantKeeperApiFactory factory = new(keyRing);
        using HttpClient client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("Cookie", cookie);

        return await client.GetAsync("/api/plants");
    }
}
