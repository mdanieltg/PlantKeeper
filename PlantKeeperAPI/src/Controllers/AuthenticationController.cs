using System.Net.Mime;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PlantKeeperAPI.Authorization;
using PlantKeeperAPI.DataTransferObjects;
using PlantKeeperAPI.Entities;
using PlantKeeperAPI.Models;

// Microsoft.AspNetCore.Mvc declares a SignInResult of its own - an ActionResult, not an
// outcome. Identity's is the one meant here.
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace PlantKeeperAPI.Controllers;

/// <summary>
/// Starting, reading and ending a session. The only controller with anonymous endpoints -
/// everything else is covered by the fallback authorization policy.
/// </summary>
[ApiController]
[Route("/api/authentication")]
[Consumes(MediaTypeNames.Application.Json)]
[Produces(MediaTypeNames.Application.Json)]
public class AuthenticationController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly UserManager<Keeper> _keepers;
    private readonly ILogger<AuthenticationController> _logger;
    private readonly SignInManager<Keeper> _signIn;

    public AuthenticationController(
        UserManager<Keeper> keepers,
        SignInManager<Keeper> signIn,
        IConfiguration configuration,
        ILogger<AuthenticationController> logger)
    {
        _keepers = keepers;
        _signIn = signIn;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>Signs in with a user name and password, and issues the session cookie.</summary>
    [HttpPost("sign-in")]
    [AllowAnonymous]
    [ProducesResponseType<SignedInKeeperDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status423Locked)]
    public async ValueTask<ActionResult<SignedInKeeperDto>> StartSession([FromBody] InputSignIn credentials)
    {
        Keeper? keeper = await _keepers.FindByNameAsync(credentials.UserName);

        // No early return on an unknown user: CheckPasswordSignInAsync is what makes a wrong
        // name and a wrong password cost the same, and skipping it here would turn the
        // timing difference into a list of which accounts exist.
        SignInResult result = keeper is null
            ? SignInResult.Failed
            : await _signIn.PasswordSignInAsync(keeper, credentials.Password, credentials.RememberMe,
                lockoutOnFailure: true);

        if (result.IsLockedOut)
        {
            _logger.LogWarning("Sign-in refused for '{UserName}': the account is locked out.", credentials.UserName);
            return Problem(
                detail: "Too many failed attempts. Try again later.",
                statusCode: StatusCodes.Status423Locked);
        }

        if (!result.Succeeded)
            return Problem(
                detail: "The user name or password is incorrect.",
                statusCode: StatusCodes.Status401Unauthorized);

        _logger.LogInformation("Keeper '{UserName}' signed in.", keeper!.UserName);

        // Read the claims off a freshly built principal rather than off HttpContext.User:
        // the cookie has been issued, but this request was authenticated before it existed,
        // so User is still anonymous until the next one.
        return Describe(keeper, await _signIn.CreateUserPrincipalAsync(keeper));
    }

    /// <summary>Who the caller is, and what the session lets them do.</summary>
    [HttpGet("me")]
    [ProducesResponseType<SignedInKeeperDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async ValueTask<ActionResult<SignedInKeeperDto>> CurrentKeeper()
    {
        Keeper? keeper = await _keepers.GetUserAsync(User);

        // Authenticated but no longer in the database. The security stamp validator normally
        // rejects that first; this covers the request that races a deletion.
        if (keeper is null) return Unauthorized();

        return Describe(keeper, User);
    }

    /// <summary>
    /// Ends the session, on this device and every other one.
    /// <para>
    /// Clearing the cookie alone would only tell the browser to forget it - the ticket
    /// inside stays signed and valid until it expires, so a copy taken beforehand still
    /// works. Rotating the security stamp is what actually revokes it, because the stamp
    /// validator re-checks every request. The cost is that signing out here signs out
    /// everywhere, which for an account with one owner is the behaviour worth having.
    /// </para>
    /// </summary>
    [HttpPost("sign-out")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async ValueTask<IActionResult> EndSession()
    {
        Keeper? keeper = await _keepers.GetUserAsync(User);
        if (keeper is not null) await _keepers.UpdateSecurityStampAsync(keeper);

        // Not SignInManager.SignOutAsync: that also signs out of the external and
        // two-factor schemes, and this app registers neither.
        await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
        return NoContent();
    }

    /// <summary>
    /// Sets the first password on a seeded account that has none.
    /// <para>
    /// Two conditions gate it, and both are one-way. The account must have no password hash
    /// - so it closes permanently the moment it succeeds - and the request must carry the
    /// <c>Bootstrap:Secret</c> value, which is set out of band through user-secrets or an
    /// environment variable and is never committed. With no secret configured the endpoint
    /// does not exist at all, which is the state a deployment should be left in.
    /// </para>
    /// </summary>
    [HttpPost("bootstrap-password")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async ValueTask<IActionResult> BootstrapPassword([FromBody] InputBootstrapPassword request)
    {
        string? secret = _configuration["Bootstrap:Secret"];
        if (string.IsNullOrEmpty(secret)) return NotFound();

        if (!FixedTimeEquals(secret, request.Secret))
        {
            _logger.LogWarning("Bootstrap password refused for '{UserName}': wrong secret.", request.UserName);
            return Problem(
                detail: "The bootstrap secret is incorrect.",
                statusCode: StatusCodes.Status401Unauthorized);
        }

        Keeper? keeper = await _keepers.FindByNameAsync(request.UserName);
        if (keeper is null) return NotFound();

        if (await _keepers.HasPasswordAsync(keeper))
            return Problem(
                detail: "This account already has a password. Change it from a signed-in session.",
                statusCode: StatusCodes.Status409Conflict);

        IdentityResult added = await _keepers.AddPasswordAsync(keeper, request.Password);
        if (!added.Succeeded)
        {
            foreach (IdentityError error in added.Errors)
                ModelState.AddModelError(nameof(request.Password), error.Description);

            return ValidationProblem(ModelState);
        }

        _logger.LogInformation("Bootstrap password set for keeper '{UserName}'.", keeper.UserName);
        return NoContent();
    }

    private static SignedInKeeperDto Describe(Keeper keeper, System.Security.Claims.ClaimsPrincipal principal) =>
        new()
        {
            Id = keeper.Id,
            UserName = keeper.UserName!,
            DisplayName = keeper.DisplayName,
            Email = keeper.Email,
            Roles = principal.FindAll(principal.Identities.First().RoleClaimType)
                .Select(claim => claim.Value)
                .Order()
                .ToArray(),
            Permissions = principal.FindAll(Permissions.ClaimType)
                .Select(claim => claim.Value)
                .Distinct()
                .Order()
                .ToArray()
        };

    /// <summary>
    /// Compares two secrets without leaking their contents through timing. Both are hashed
    /// first so the comparison itself sees equal-length inputs - FixedTimeEquals returns
    /// early on a length mismatch, which would otherwise leak the expected length.
    /// </summary>
    private static bool FixedTimeEquals(string expected, string provided) =>
        CryptographicOperations.FixedTimeEquals(
            SHA256.HashData(Encoding.UTF8.GetBytes(expected)),
            SHA256.HashData(Encoding.UTF8.GetBytes(provided)));
}
