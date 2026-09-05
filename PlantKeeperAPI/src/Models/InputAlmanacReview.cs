using System.ComponentModel.DataAnnotations;

namespace PlantKeeperAPI.Models;

/// <summary>A moderator's decision on a queued almanac change.</summary>
public class InputAlmanacReview
{
    /// <summary>Why. Optional on an approval, and the courteous thing on a rejection.</summary>
    [StringLength(500)]
    public string? Note { get; set; }
}
