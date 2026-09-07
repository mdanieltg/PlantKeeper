using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using PlantKeeperAPI.Authorization;
using PlantKeeperAPI.Entities;

namespace PlantKeeperAPI.Extensions;

public static class AuthenticationServiceExtensions
{
    /// <summary>
    /// Cookie authentication over Identity's application scheme.
    /// <para>
    /// The cookie handler's defaults are built for server-rendered pages: an unauthenticated
    /// request gets a 302 to a login page and a forbidden one a 302 to an access-denied
    /// page. For an API both are wrong - a fetch follows the redirect and reports 200 with
    /// an HTML body - so the redirect events are replaced with plain status codes.
    /// </para>
    /// </summary>
    public static IServiceCollection AddCookieAuthentication(
        this IServiceCollection services,
        IWebHostEnvironment environment)
    {
        services
            .AddAuthentication(IdentityConstants.ApplicationScheme)
            .AddIdentityCookies();

        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.Name = "plantkeeper.session";
            options.Cookie.HttpOnly = true;

            // Strict, not the usual Lax. There is no antiforgery token anywhere in this
            // API, so the cookie's own send rules are a load-bearing CSRF defence rather
            // than a nicety. Strict costs nothing here: nginx serves a static document, and
            // the SPA's own /api calls afterwards are same-site, so they still carry the
            // cookie. It would only bite on a route that needs the session at
            // document-request time after an off-site click, and none exists.
            options.Cookie.SameSite = SameSiteMode.Strict;

            // In production the app and the API are one origin behind nginx, which
            // terminates TLS; SameAsRequest in development keeps plain-HTTP localhost
            // working without weakening the deployed cookie.
            options.Cookie.SecurePolicy = environment.IsDevelopment()
                ? CookieSecurePolicy.SameAsRequest
                : CookieSecurePolicy.Always;

            options.ExpireTimeSpan = TimeSpan.FromDays(14);
            options.SlidingExpiration = true;

            options.Events.OnRedirectToLogin = context =>
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            };

            options.Events.OnRedirectToAccessDenied = context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            };
        });

        // AddIdentityCookies wires the application cookie's OnValidatePrincipal to
        // SecurityStampValidator, which resolves this from the container. AddIdentityCore
        // does not register it - only the full AddIdentity does - so it is registered here
        // or every request fails resolving it.
        services.AddScoped<ISecurityStampValidator, SecurityStampValidator<Keeper>>();
        services.AddScoped<ITwoFactorSecurityStampValidator, TwoFactorSecurityStampValidator<Keeper>>();

        services.Configure<SecurityStampValidatorOptions>(options =>
            // Revalidate on every request rather than the default half hour. The validator
            // rebuilds the principal from the database when it runs, so this is what makes a
            // role or permission change take effect on the next request instead of at the
            // next sign-in. It costs one user lookup per request, which this app can afford.
            options.ValidationInterval = TimeSpan.Zero);

        return services;
    }

    /// <summary>
    /// Where the DataProtection key ring lives - the keys that sign and encrypt the
    /// session cookie.
    /// <para>
    /// Unconfigured, the framework writes the ring under the user profile, which inside a
    /// container is a directory in the writable layer. Every redeploy then starts with an
    /// empty ring, mints a new key, and every outstanding <c>plantkeeper.session</c> cookie
    /// stops decrypting - a deploy signs everybody out, silently and only for the people
    /// who were already signed in. Pointing it at a mounted volume is what survives.
    /// </para>
    /// <para>
    /// <c>SetApplicationName</c> is the other half. The default discriminator is derived
    /// from the content root path, so keys written by a container that unpacked the app
    /// somewhere else would be present on the volume and still refuse to decrypt. Pinning
    /// the name makes the ring portable across image rebuilds.
    /// </para>
    /// <para>
    /// The ring is written unencrypted - there is no DPAPI on Linux and no certificate
    /// configured, which the key manager says out loud at startup: <c>No XML encryptor
    /// configured</c>. Whoever can read the volume can forge a session cookie, so the
    /// volume's own access control is what protects it.
    /// </para>
    /// <para>
    /// Registering is side-effect free when the path is unset; refusing to run without one
    /// is <see cref="RequireSessionKeyRing" />, on the pipeline side. That split is not
    /// cosmetic - see the remarks there.
    /// </para>
    /// </summary>
    public static IServiceCollection AddSessionKeyRing(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        string? keyPath = configuration["DataProtection:KeyPath"];

        // Unset is not decided here. Development wants the framework default - `dotnet run`
        // on a real user profile, where it already persists between runs - and everything
        // else is refused by RequireSessionKeyRing once the app actually starts.
        if (string.IsNullOrWhiteSpace(keyPath)) return services;

        // Created here rather than left to the repository, so a path that cannot be written
        // fails at startup naming the directory instead of at the first key rotation.
        DirectoryInfo keyRing = Directory.CreateDirectory(keyPath);

        services
            .AddDataProtection()
            .PersistKeysToFileSystem(keyRing)
            .SetApplicationName("PlantKeeper");

        return services;
    }

    /// <summary>
    /// Default-deny authorization plus the permission policies.
    /// <para>
    /// The fallback policy applies to every endpoint that carries no authorization metadata
    /// of its own, so a controller added later is protected without anyone remembering to
    /// protect it. Opting out is explicit and greppable: <c>[AllowAnonymous]</c>.
    /// </para>
    /// </summary>
    public static IServiceCollection AddPermissionAuthorization(this IServiceCollection services)
    {
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();

        services.AddAuthorization(options =>
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build());

        return services;
    }

    /// <summary>
    /// Restores the client's scheme and address from nginx's <c>X-Forwarded-*</c> headers.
    /// <para>
    /// First in the pipeline, before anything reads the scheme. Behind nginx every request
    /// arrives over plain HTTP on the container network, so without this
    /// <c>Request.IsHttps</c> is false, a Secure cookie is never returned to the browser,
    /// and sign-in appears to succeed while no session is ever established.
    /// </para>
    /// </summary>
    public static WebApplication UseProxyHeaders(this WebApplication app)
    {
        ForwardedHeadersOptions forwardedHeaders = new()
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
        };

        // The defaults trust loopback only, and nginx is a different container on
        // plantkeeper-net, so the headers would be dropped unread. Clearing the lists means
        // trusting whatever sends them - safe only because the API port is never published:
        // nginx is the sole route to it. Publish the backend and this becomes a header
        // spoofing hole.
        forwardedHeaders.KnownIPNetworks.Clear();
        forwardedHeaders.KnownProxies.Clear();

        app.UseForwardedHeaders(forwardedHeaders);

        return app;
    }

    /// <summary>
    /// Refuses to serve outside Development without a persistent key ring.
    /// <para>
    /// This runs after <c>builder.Build()</c> on purpose, and the reason is
    /// <c>dotnet ef</c>. The EF tools build the application's whole host to find the
    /// DbContext, so anything that throws while services are being registered takes the
    /// migration command down with it - and EF reports its own downstream failure,
    /// <c>The ConnectionString property has not been initialized</c>, which names nothing
    /// about the actual cause. Design-time host building never gets past the built host,
    /// so a check on this side lets migrations run while a real deployment still refuses.
    /// </para>
    /// <para>
    /// Refusing at all is deliberate: the failure it prevents is invisible. Without a
    /// persistent ring the app runs perfectly and sessions simply do not survive a deploy,
    /// which nobody sees until somebody who was signed in comes back.
    /// </para>
    /// </summary>
    public static WebApplication RequireSessionKeyRing(this WebApplication app)
    {
        if (app.Environment.IsDevelopment()) return app;

        if (string.IsNullOrWhiteSpace(app.Configuration["DataProtection:KeyPath"]))
            throw new InvalidOperationException(
                "DataProtection:KeyPath is not configured. Set it to a directory on a "
                + "persistent volume - see ci/docker-compose.prod.yml - or every deployment "
                + "will issue new keys and sign out every existing session.");

        return app;
    }

    /// <summary>
    /// Authenticates the request, then authorizes it. Runs after CORS, so a rejected
    /// cross-origin request still carries the headers that let the browser report why.
    /// </summary>
    public static WebApplication UseAuthenticationPipeline(this WebApplication app)
    {
        app.UseAuthentication();
        app.UseAuthorization();

        return app;
    }
}
