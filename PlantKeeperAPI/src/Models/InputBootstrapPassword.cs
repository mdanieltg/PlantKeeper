using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace PlantKeeperAPI.Models;

/// <summary>
/// The one-time claim of the seeded keeper account. See
/// <c>AuthenticationController.BootstrapPassword</c> for the two conditions that have to
/// hold for it to be accepted.
/// </summary>
[DebuggerDisplay("BootstrapPassword: {UserName}")]
public class InputBootstrapPassword
{
    [StringLength(256, MinimumLength = 1)]
    public required string UserName { get; set; }

    /// <summary>Matched against <c>Bootstrap:Secret</c>, which is set out of band.</summary>
    public required string Secret { get; set; }

    [StringLength(256, MinimumLength = 12)]
    public required string Password { get; set; }
}
