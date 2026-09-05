using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace PlantKeeperAPI.Models;

[DebuggerDisplay("SignIn: {UserName}")]
public class InputSignIn
{
    [StringLength(256, MinimumLength = 1)]
    public required string UserName { get; set; }

    // No StringLength: the length rule belongs to the password policy applied when a
    // password is set. Validating it here would tell an attacker which guesses were the
    // wrong shape before any of them reached the hasher.
    public required string Password { get; set; }

    /// <summary>Keeps the session across browser restarts.</summary>
    public bool RememberMe { get; set; }
}
