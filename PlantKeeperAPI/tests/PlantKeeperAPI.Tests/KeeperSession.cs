using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PlantKeeperAPI.Tests;

/// <summary>
/// One signed-in keeper's HTTP client, with the session cookie attached to every request.
/// <para>
/// The cookie is carried by hand rather than by a <c>CookieContainer</c>: the sign-in
/// response is the only place it appears, and reading it here means a test that stops
/// getting one fails on the assertion below rather than on a later 401 that looks like an
/// authorization bug.
/// </para>
/// </summary>
public sealed class KeeperSession : IDisposable
{
    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private KeeperSession(HttpClient client, Guid keeperId)
    {
        Client = client;
        KeeperId = keeperId;
    }

    public HttpClient Client { get; }

    /// <summary>The signed-in keeper's id, as the API itself reports it.</summary>
    public Guid KeeperId { get; }

    public void Dispose() => Client.Dispose();

    public static async Task<KeeperSession> SignInAsync(PlantKeeperApiFactory factory, string userName)
    {
        HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/authentication/sign-in",
            new { userName, password = TestKeepers.Password, rememberMe = false });

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException(
                $"Could not sign in as '{userName}': {(int)response.StatusCode} " +
                $"{await response.Content.ReadAsStringAsync()}");

        string cookie = response.Headers.GetValues("Set-Cookie")
            .Select(header => header.Split(';')[0])
            .Single(value => value.StartsWith("plantkeeper.session=", StringComparison.Ordinal));

        client.DefaultRequestHeaders.Add("Cookie", cookie);

        SignedInKeeper? keeper = await response.Content.ReadFromJsonAsync<SignedInKeeper>(Json);
        return new KeeperSession(client, keeper!.Id);
    }

    public Task<HttpResponseMessage> GetAsync(string path) => Client.GetAsync(path);

    public Task<HttpResponseMessage> PostAsync(string path, object body) =>
        Client.PostAsJsonAsync(path, body, Json);

    public Task<HttpResponseMessage> PutAsync(string path, object body) =>
        Client.PutAsJsonAsync(path, body, Json);

    public Task<HttpResponseMessage> DeleteAsync(string path) => Client.DeleteAsync(path);

    public async Task<TResponse> ReadAsync<TResponse>(string path)
    {
        HttpResponseMessage response = await GetAsync(path);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<TResponse>(Json))!;
    }

    /// <summary>Posts, insists on the status it expects, and hands back the created id.</summary>
    public async Task<Guid> CreateAsync(string path, object body)
    {
        HttpResponseMessage response = await PostAsync(path, body);

        if (response.StatusCode != System.Net.HttpStatusCode.Created)
            throw new InvalidOperationException(
                $"POST {path} returned {(int)response.StatusCode}: {await response.Content.ReadAsStringAsync()}");

        Created? created = await response.Content.ReadFromJsonAsync<Created>(Json);
        return created!.Id;
    }

    private sealed record Created(Guid Id);

    public sealed record SignedInKeeper(Guid Id, string UserName, string[] Roles, string[] Permissions);
}
