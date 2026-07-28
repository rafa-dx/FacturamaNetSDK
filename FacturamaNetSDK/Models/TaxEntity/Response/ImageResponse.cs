namespace FacturamaNetSDK.Models.TaxEntity.Response;

/// <summary>
/// Respuesta al cargar una imagen (logo) del perfil fiscal.
/// </summary>
public sealed record ImageResponse
{
    /// <summary>URL o contenido de la imagen procesada.</summary>
    public string Image { get; init; } = string.Empty;

    /// <summary>Mensaje devuelto por la API.</summary>
    public string Message { get; init; } = string.Empty;
}
