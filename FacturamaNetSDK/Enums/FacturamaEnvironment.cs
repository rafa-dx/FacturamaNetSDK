namespace FacturamaNetSDK.Enums
{
    public enum FacturamaEnvironment
    {
        /// <summary>
        /// Ambiente de pruebas (Sandbox).
        /// Usar este ambiente para desarrollo y testing.
        /// URL: https://api.facturama.mx/api
        /// </summary>
        Sandbox = 0,

        /// <summary>
        /// Ambiente de producción.
        /// Usar este ambiente para operaciones reales de facturación.
        /// URL: https://api.facturama.mx/api
        /// </summary>
        Production = 1
    }


    /// <summary>
    /// Extensiones para el enum FacturamaEnvironment.
    /// </summary>
    public static class FacturamaEnvironmentExtensions
    {
        /// <summary>
        /// Obtiene la URL base de la API según el ambiente.
        /// </summary>
        /// <param name="environment">Ambiente de Facturama.</param>
        /// <returns>URL base de la API.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Se lanza cuando el ambiente no es válido.
        /// </exception>
        public static string GetBaseUrl(this FacturamaEnvironment environment)
        {
            return environment switch
            {
                FacturamaEnvironment.Sandbox => "https://apisandbox.facturama.mx",
                FacturamaEnvironment.Production => "https://api.facturama.mx",
                _ => throw new ArgumentOutOfRangeException(
                    nameof(environment),
                    environment,
                    "El ambiente especificado no es válido")
            };
        }

        /// <summary>
        /// Obtiene el nombre descriptivo del ambiente.
        /// </summary>
        /// <param name="environment">Ambiente de Facturama.</param>
        /// <returns>Nombre descriptivo del ambiente.</returns>
        public static string GetDisplayName(this FacturamaEnvironment environment)
        {
            return environment switch
            {
                FacturamaEnvironment.Sandbox => "Sandbox (Pruebas)",
                FacturamaEnvironment.Production => "Producción",
                _ => "Desconocido"
            };
        }

        /// <summary>
        /// Indica si el ambiente es de producción.
        /// </summary>
        /// <param name="environment">Ambiente de Facturama.</param>
        /// <returns>True si es producción, false si es sandbox.</returns>
        public static bool IsProduction(this FacturamaEnvironment environment)
        {
            return environment == FacturamaEnvironment.Production;
        }

        /// <summary>
        /// Indica si el ambiente es de pruebas (sandbox).
        /// </summary>
        /// <param name="environment">Ambiente de Facturama.</param>
        /// <returns>True si es sandbox, false si es producción.</returns>
        public static bool IsSandbox(this FacturamaEnvironment environment)
        {
            return environment == FacturamaEnvironment.Sandbox;
        }


    }
}
