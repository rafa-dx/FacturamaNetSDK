namespace FacturamaNetSDK.Configuration
{
    /// <summary>
    /// Configuración de la política de reintentos ante errores transitorios.
    /// </summary>
    public sealed record RetryOptions
    {
        /// <summary>Activa o desactiva los reintentos globalmente. Default: true.</summary>
        public bool Enabled { get; init; } = true;

        /// <summary>Número máximo de reintentos. Default: 3.</summary>
        public int MaxRetries { get; init; } = 3;

        /// <summary>Retardo base del backoff exponencial (base^intento). Default: 2s.</summary>
        public TimeSpan BaseDelay { get; init; } = TimeSpan.FromSeconds(2);

        // --- Reintentos por verbo HTTP ---

        /// <summary>Reintentar GET (idempotente). Default: true.</summary>
        public bool RetryGet { get; init; } = true;

        /// <summary>Reintentar POST. Default: false .</summary>
        public bool RetryPost { get; init; } = false;

        /// <summary>Reintentar PUT (idempotente). Default: true.</summary>
        public bool RetryPut { get; init; } = true;

        /// <summary>Reintentar DELETE (idempotente). Default: true.</summary>
        public bool RetryDelete { get; init; } = true;

        /// <summary>
        /// Indica si un método HTTP debe reintentarse según la configuración por verbo.
        /// El apagado global (<see cref="Enabled"/>) se resuelve en la fábrica de clientes HTTP.
        /// </summary>
        internal bool ShouldRetry(HttpMethod method) =>
            method.Method switch
            {
                "GET" => RetryGet,
                "POST" => RetryPost,
                "PUT" => RetryPut,
                "DELETE" => RetryDelete,
                _ => false
            };
    }
}
