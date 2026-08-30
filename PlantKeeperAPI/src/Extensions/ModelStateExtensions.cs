using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;

namespace PlantKeeperAPI.Extensions;

/// <summary>
/// Foreign-key existence checks. The database rejects a bad key with a 500-shaped
/// <see cref="DbUpdateException" />; these turn it into a 422 naming the offending field.
/// </summary>
public static partial class ModelStateExtensions
{
    public static async ValueTask<bool> RequireExistsAsync<TEntity>(this ModelStateDictionary modelState,
        DbContext dbContext, Guid id, string key) where TEntity : class
    {
        if (await dbContext.Set<TEntity>().FindAsync(id) is not null) return true;

        modelState.TryAddModelError(key, $"The {Describe<TEntity>()} provided does not exist.");
        return false;
    }

    /// <summary>Optional foreign key - a null id is valid and skips the lookup.</summary>
    public static async ValueTask<bool> RequireExistsAsync<TEntity>(this ModelStateDictionary modelState,
        DbContext dbContext, Guid? id, string key) where TEntity : class =>
        id is null || await modelState.RequireExistsAsync<TEntity>(dbContext, id.Value, key);

    /// <summary>Validates every id in a replace-the-whole-set request body.</summary>
    public static async ValueTask<bool> RequireAllExistAsync<TEntity>(this ModelStateDictionary modelState,
        DbContext dbContext, IEnumerable<Guid> ids, string key) where TEntity : class
    {
        bool valid = true;

        foreach (Guid id in ids.Distinct())
        {
            if (await dbContext.Set<TEntity>().FindAsync(id) is not null) continue;

            modelState.TryAddModelError(key, $"The {Describe<TEntity>()} '{id}' does not exist.");
            valid = false;
        }

        return valid;
    }

    private static string Describe<TEntity>() =>
        PascalCaseBoundary().Replace(typeof(TEntity).Name, " $1").ToLowerInvariant();

    [GeneratedRegex("(?<!^)([A-Z])")]
    private static partial Regex PascalCaseBoundary();
}
