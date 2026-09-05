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

    /// <summary>
    /// Ignores <see cref="IKeeperOwned.KeeperId" /> on the destination.
    /// <para>
    /// Ownership is never accepted from a request body - the controller sets it from the
    /// signed-in principal - so no <c>Input</c> model has a source for it. Without this the
    /// strict mapping configuration refuses to compile the pair at startup, which is the
    /// intended safety net rather than an inconvenience: it means a new owned entity cannot
    /// silently take its owner from the client.
    /// </para>
    /// </summary>
    public static TypeAdapterSetter<TSource, TDestination> IgnoreOwnership<TSource, TDestination>(
        this TypeAdapterSetter<TSource, TDestination> setter)
        where TDestination : IKeeperOwned =>
        setter.Ignore(nameof(IKeeperOwned.KeeperId));

    /// <summary>
    /// Ignores <see cref="IAlmanacVersioned.Version" /> on the destination.
    /// <para>
    /// The version belongs to the database, not to the request: it is bumped in
    /// <c>SaveChanges</c> and a body that could set it could also forge one, defeating the
    /// staleness check a proposal is approved against. Same shape as
    /// <see cref="IgnoreOwnership{TSource,TDestination}" /> and for the same reason.
    /// </para>
    /// </summary>
    public static TypeAdapterSetter<TSource, TDestination> IgnoreVersion<TSource, TDestination>(
        this TypeAdapterSetter<TSource, TDestination> setter)
        where TDestination : IAlmanacVersioned =>
        setter.Ignore(nameof(IAlmanacVersioned.Version));

    private static bool IsNavigation(Type type)
    {
        if (IsEntity(type)) return true;

        // Collection navigations - List<Plant> and friends.
        if (type == typeof(string) || !typeof(IEnumerable).IsAssignableFrom(type)) return false;

        return type.IsGenericType && type.GetGenericArguments().Any(IsEntity);
    }

    private static bool IsEntity(Type type) => type.Namespace == EntityNamespace;
}
