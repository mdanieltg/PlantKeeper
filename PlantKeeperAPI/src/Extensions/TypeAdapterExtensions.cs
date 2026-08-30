using System.Collections;
using Mapster;
using PlantKeeperAPI.Entities;

namespace PlantKeeperAPI.Extensions;

public static class TypeAdapterExtensions
{
    private static readonly string EntityNamespace = typeof(Plant).Namespace!;

    /// <summary>
    /// Ignores every navigation property on the destination - reference navigations and
    /// collections alike. Mapping runs with <c>RequireDestinationMemberSource</c> on, so
    /// each Input-to-Entity pair would otherwise need a hand-written
    /// <c>Ignore</c> for each one.
    /// </summary>
    public static TypeAdapterSetter<TSource, TDestination> IgnoreNavigations<TSource, TDestination>(
        this TypeAdapterSetter<TSource, TDestination> setter)
    {
        string[] navigations = typeof(TDestination)
            .GetProperties()
            .Where(property => IsNavigation(property.PropertyType))
            .Select(property => property.Name)
            .ToArray();

        if (navigations.Length > 0) setter.Ignore(navigations);

        return setter;
    }

    private static bool IsNavigation(Type type)
    {
        if (IsEntity(type)) return true;

        // Collection navigations - List<Plant> and friends.
        if (type == typeof(string) || !typeof(IEnumerable).IsAssignableFrom(type)) return false;

        return type.IsGenericType && type.GetGenericArguments().Any(IsEntity);
    }

    private static bool IsEntity(Type type) => type.Namespace == EntityNamespace;
}
