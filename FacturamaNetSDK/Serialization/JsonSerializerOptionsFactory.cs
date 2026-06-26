using System.Text.Json;
using System.Text.Json.Serialization;

namespace FacturamaNetSDK.Serialization;

/// <summary>
/// Fábrica de opciones de serialización JSON para el SDK.
/// </summary>
internal static class JsonSerializerOptionsFactory
{
    private static readonly JsonSerializerOptions _default = CreateDefault();

    /// <summary>
    /// Opciones por defecto para serialización/deserialización.
    /// </summary>
    internal static JsonSerializerOptions Default => _default;

    private static JsonSerializerOptions CreateDefault()
    {
        var options = new JsonSerializerOptions
        {
            // La API devuelve PascalCase — insensible a mayúsculas cubre casos mixtos
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
            WriteIndented = false
        };

        options.Converters.Add(new JsonStringEnumConverter());

        return options;
    }
}

