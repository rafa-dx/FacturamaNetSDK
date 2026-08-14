using System.Text.Json.Serialization;

namespace FacturamaNetSDK.Models.Filters
{
    /// <summary>
    /// Opciones de paginación y búsqueda para listados paginados.
    /// </summary>
    public sealed record QueryOptions
    {
        /// <summary>
        /// Índice del primer registro a devolver (base 0). Default: 0.
        /// </summary>
        [JsonPropertyName("start")]
        public int Start { get; init; } = 0;

        /// <summary>
        /// Cantidad máxima de registros a devolver. Default: 10.
        /// </summary>
        [JsonPropertyName("length")]
        public int Length { get; init; } = 10;

        /// <summary>
        /// Texto de búsqueda libre. Vacío devuelve todos los registros.
        /// </summary>
        [JsonPropertyName("search")]
        public string Search { get; init; } = string.Empty;
    }
}
