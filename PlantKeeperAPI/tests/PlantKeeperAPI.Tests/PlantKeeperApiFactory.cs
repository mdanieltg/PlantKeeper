using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace PlantKeeperAPI.Tests;

/// <summary>
/// Boots the real API in memory - the real pipeline, the real authorization, the real
/// query filters - against <see cref="TestDatabase" />.
/// <para>
/// Nothing is stubbed. A tenancy leak is a property of the EF model and the middleware
/// order rather than of any one class, so replacing either would test something other than
/// the thing at risk.
/// </para>
/// </summary>
public class PlantKeeperApiFactory : WebApplicationFactory<Program>
{
    private readonly string? _keyRingPath;

    /// <param name="keyRingPath">
    /// Where DataProtection writes the keys that sign the session cookie. Left unset by
    /// every test but <see cref="SessionKeyRingTests" />, which is the only one that cares
    /// where they live - and cares enough to boot two hosts over the same directory.
    /// </param>
    public PlantKeeperApiFactory(string? keyRingPath = null) => _keyRingPath = keyRingPath;

    protected override IHost CreateHost(IHostBuilder builder)
    {
        // Development, deliberately: DatabaseServiceExtensions picks the connection-string
        // key from the environment name, so this is what makes ConnectionStrings:Dev the
        // key the app reads.
        builder.UseEnvironment(Environments.Development);

        builder.ConfigureHostConfiguration(configuration => configuration.AddInMemoryCollection(
            new Dictionary<string, string?>
            {
                ["ConnectionStrings:Dev"] = TestDatabase.ConnectionString,

                // Closed. The tests set passwords through UserManager instead, so leaving
                // the bootstrap endpoint open would only widen what they exercise.
                ["Bootstrap:Secret"] = string.Empty,

                ["DataProtection:KeyPath"] = _keyRingPath
            }));

        return base.CreateHost(builder);
    }
}
