namespace FacturamaNetSDK.Configuration
{
    /// <summary>
    /// Configuración de la política de reintentos ante errores transitorios.
    /// </summary>
    public sealed record RetryOptions
    {
        /// <summary>
        /// Tope de <see cref="MaxRetries"/>. Con el default de 2s de <see cref="BaseDelay"/>,
        /// el décimo reintento ya espera más de 17 minutos.
        /// </summary>
        public const int MaxRetriesLimit = 10;

        /// <summary>Activa o desactiva los reintentos globalmente. Default: true.</summary>
        public bool Enabled { get; init; } = true;

        /// <summary>
        /// Número máximo de reintentos, sin contar el intento inicial. Default: 3.
        /// <para>
        /// ⚠️ <b>A definir con el equipo.</b> Admite de 0 a <see cref="MaxRetriesLimit"/>;
        /// el tope evita que el backoff exponencial desborde el presupuesto de la operación.
        /// </para>
        /// </summary>
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
        /// <summary>
        /// Intentos que puede consumir una sola operación (el inicial más los reintentos).
        /// Vale 1 cuando los reintentos están apagados o ningún verbo los tiene habilitados.
        /// El circuit breaker usa este valor para dimensionar su umbral.
        /// </summary>
        internal int MaxAttemptsPerOperation =>
            Enabled && (RetryGet || RetryPost || RetryPut || RetryDelete)
                ? MaxRetries + 1
                : 1;

        internal void Validate()
        {
            if (!Enabled)
                return;

            if (MaxRetries < 0 || MaxRetries > MaxRetriesLimit)
                throw new ArgumentOutOfRangeException(
                    nameof(MaxRetries),
                    MaxRetries,
                    $"Debe estar entre 0 y {MaxRetriesLimit}. Usa Enabled = false para desactivar los reintentos.");

            if (BaseDelay <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(
                    nameof(BaseDelay),
                    BaseDelay,
                    "Debe ser mayor a cero. Un retardo de cero martillea un servicio caído.");
        }
    }
}
