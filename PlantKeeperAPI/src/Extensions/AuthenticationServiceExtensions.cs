using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
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
