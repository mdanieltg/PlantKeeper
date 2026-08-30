using System.Text.Json;
using System.Text.Json.Serialization;

namespace PlantKeeperAPI.Extensions;

public static class JsonSerializerExtensions
{
    /// <summary>
    /// Applies the serialization rules the API depends on. This has to be applied to both
    /// the MVC options and the <c>Http.Json</c> options: MVC's govern what the endpoints
    /// actually serialize, while the OpenAPI document generator reads the
    /// <c>Http.Json</c> set. Configure only the first and the published schema advertises
    /// integer enums while the API returns strings.
    /// </summary>
    public static JsonSerializerOptions ApplyPlantKeeperDefaults(this JsonSerializerOptions options)
    {
        // The enums are closed scales persisted as varchar, so an out-of-range integer
        // must be rejected rather than written through as "99".
        options.Converters.Add(new JsonStringEnumConverter(namingPolicy: null, allowIntegerValues: false));

        return options;
    }
}
