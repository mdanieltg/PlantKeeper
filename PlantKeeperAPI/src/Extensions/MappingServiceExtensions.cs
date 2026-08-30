using System.Reflection;
using Mapster;
using MapsterMapper;

namespace PlantKeeperAPI.Extensions;

public static class MappingServiceExtensions
{
    /// <summary>
    /// Registers Mapster from the <see cref="IRegister" /> classes in this assembly.
    /// <para>
    /// Two settings turn a whole class of silent bug loud. <c>RequireDestinationMemberSource</c>
    /// makes a destination member without a source an error rather than a default value,
    /// and <c>Compile</c> raises it at startup instead of on the first request that
    /// happens to hit the mapping. Convention-based mapping used to leave empty strings
    /// and <c>Guid.Empty</c> in responses from code that compiled clean.
    /// </para>
    /// </summary>
    public static IServiceCollection AddMapping(this IServiceCollection services)
    {
        TypeAdapterConfig config = TypeAdapterConfig.GlobalSettings;

        config.Default.RequireDestinationMemberSource(true);
        config.Scan(Assembly.GetExecutingAssembly());
        config.Compile();

        services.AddSingleton(config);
        services.AddScoped<IMapper, ServiceMapper>();

        return services;
    }
}
