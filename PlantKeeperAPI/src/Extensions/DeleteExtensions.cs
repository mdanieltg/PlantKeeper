using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace PlantKeeperAPI.Extensions;

/// <summary>
/// Deleting a row the almanac still points at.
/// <para>
/// Since <c>RestrictAlmanacDeletes</c>, removing a shared row that something still
/// references is refused by the database rather than silently cascading through other
/// keepers' collections. Left alone that surfaces as a 500; these turn it into a 409 that
/// says what is in the way.
/// </para>
/// </summary>
public static partial class DeleteExtensions
{
    /// <summary>PostgreSQL <c>foreign_key_violation</c>.</summary>
    private const string ForeignKeyViolation = "23503";

    /// <summary>
    /// Removes an entity and saves, returning <c>null</c> on success or a 409 result when
    /// something still references it. Callers read as
    /// <c>return await this.DeleteAsync(_dbContext, row) ?? NoContent();</c>.
    /// </summary>
    public static async ValueTask<IActionResult?> DeleteAsync(
        this ControllerBase controller,
        DbContext dbContext,
        object entity)
    {
        dbContext.Remove(entity);

        try
        {
            await dbContext.SaveChangesAsync();
            return null;
        }
        catch (DbUpdateException exception)
            when (exception.InnerException is PostgresException { SqlState: ForeignKeyViolation } violation)
        {
            // The entity is still tracked as Deleted, and this DbContext is scoped to the
            // request - but detaching keeps a later SaveChanges in the same request from
            // retrying the delete that just failed.
            dbContext.Entry(entity).State = EntityState.Unchanged;

            return controller.Conflict(StillInUse(violation.TableName));
        }
    }

    /// <summary>
    /// Names what is in the way. On a delete, PostgreSQL reports the <em>referencing</em>
    /// table in <see cref="PostgresException.TableName" />, which is the useful half - the
    /// caller already knows what they tried to delete.
    /// <para>
    /// Public because <c>PlantSpeciesService</c> owns its own delete and needs the same body.
    /// </para>
    /// </summary>
    public static ProblemDetails StillInUse(string? referencingTable)
    {
        ProblemDetails problem = new()
        {
            Status = StatusCodes.Status409Conflict,
            Title = "Still in use",
            Detail = $"This row cannot be deleted while {Humanize(referencingTable)} still reference it. " +
                     "Reassign or remove those first."
        };

        // The raw table name, for a client that wants to route the user somewhere useful
        // rather than only show the sentence.
        problem.Extensions["referencedBy"] = referencingTable;

        return problem;
    }

    private static string Humanize(string? tableName) =>
        string.IsNullOrEmpty(tableName)
            ? "other rows"
            : PascalCaseBoundary().Replace(tableName, " $1").ToLowerInvariant().Trim();

    [GeneratedRegex("(?<!^)([A-Z])")]
    private static partial Regex PascalCaseBoundary();
}
