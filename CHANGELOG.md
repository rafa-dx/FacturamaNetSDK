# Changelog

Todos los cambios notables de este proyecto se documentan aquí.
El formato se basa en [Keep a Changelog](https://keepachangelog.com/es-ES/1.0.0/)
y el versionado sigue [SemVer](https://semver.org/lang/es/).

## [Sin publicar]

### Añadido
- **`FacturamaOptions.BaseUrlOverride`** para apuntar el SDK a un servidor local (mock server,
  WireMock, contenedor) sin tocar `Environment`, que queda ignorado cuando el override está
  presente. Se valida al construir el cliente —no en la primera petición— y exige URL absoluta
  con esquema http/https; `http://` solo se admite en loopback, porque Basic Auth viaja en
  base64 y apuntar a un host remoto sin TLS expondría las credenciales de la cuenta.
- Sobrecarga `FacturamaClient(username, password, environment, logger)`: el ambiente explícito
  ya estaba documentado como parte del entry point pero no existía en la superficie pública.
- **Multi-targeting `netstandard2.0;net8.0`** (antes solo `net6.0`). El paquete ahora cubre
  .NET Framework 4.6.1+, .NET Core 2.0+ y .NET 5–7 por la vía de `netstandard2.0`, además de
  .NET 8+ con su propio binario. `net6.0` se deja de compilar explícitamente: está fuera de
  soporte desde noviembre de 2024 y esos consumidores resuelven el asset `netstandard2.0`.
  - `LangVersion` fijado en **10.0** para ambos targets. Sin fijarlo, `netstandard2.0` usaría
    C# 7.3 (donde ni `Nullable` compila) y `net8.0` C# 12: un solo valor evita que el código
    divergiera por target.
  - `System.Runtime.CompilerServices.IsExternalInit` como polyfill interno
    (`Compatibility/IsExternalInit.cs`): habilita los `record` y las 371 propiedades `init`
    de los modelos en `netstandard2.0`.
  - `Internal/Guard.NotNull` reemplaza los 14 usos de `ArgumentNullException.ThrowIfNull`
    (net6+) en los endpoints y en `FacturamaClient`.
  - `Internal/HttpContentExtensions` encapsula la lectura del cuerpo de la respuesta: las
    sobrecargas con `CancellationToken` de `HttpContent` son net5+. ⚠️ En `netstandard2.0` el
    token se comprueba antes de leer pero **no interrumpe una lectura en curso**; ahí el corte
    lo pone el `Timeout` del `HttpClient`.
  - `System.Text.Json` 8.0.6 como dependencia **solo** de `netstandard2.0`.
  - Verificado en runtime: el binario `netstandard2.0` ejecuta el pipeline HTTP completo
    (Basic Auth, Polly, breaker, traducción de excepciones), los `record` con `init` y
    `System.Text.Json` sobre .NET Framework 4.8.
- **Circuit breaker en dos capas encadenadas.** A la de racha existente (`CircuitBreakerAsync`)
  se suma una de proporción (`AdvancedCircuitBreakerAsync`): `FailureRatio`, `SamplingDuration`
  y `MinimumThroughput` en `CircuitBreakerOptions`. Cada capa cubre el punto ciego de la otra —
  la racha protege al consumidor de bajo volumen, el ratio detecta degradación parcial bajo
  carga. Cualquiera de las dos abre el circuito y ambas comparten `BreakDuration`.
- Validación cruzada entre `CircuitBreakerOptions` y `RetryOptions`: `FailuresBeforeBreaking` y
  `MinimumThroughput` deben superar `MaxRetries + 1`. Ambas capas cuentan intentos y no
  operaciones, así que un umbral por debajo hacía que una única petición fallida dejara el
  circuito abierto para toda la cuenta.
- Proyecto de pruebas: cobertura de la validación de opciones y del comportamiento de las dos
  capas del breaker (apertura, ventana deslizante, estado compartido entre verbos, half-open).
- Documentación XML (`GenerateDocumentationFile`) y metadatos de paquete NuGet en el `.csproj`.
- README con inicio rápido, manejo de errores y ejemplos reales.
- Este CHANGELOG.
- Comentarios XML en los modelos de request principales (CfdiRequest, Item, Receiver, Issuer).

### Cambiado
- **Default de `CircuitBreakerOptions.FailuresBeforeBreaking`: 5 → 10.** Con 4 intentos por
  operación, el valor anterior abría el circuito a mitad de la segunda operación fallida.
  Al haber ahora una segunda capa que cubre la degradación parcial, ninguna necesita ser
  agresiva. ⚠️ Umbral a definir con el equipo.
- `CircuitBreakerOptions` pasó de `class` a `record` (consistente con `RetryOptions`) y recibió
  documentación XML completa.
- `RetryOptions.validate()` → `Validate()`, y ahora valida de verdad: `MaxRetries` entre 0 y 10
  (el tope evita que el backoff exponencial desborde el `TimeSpan` del presupuesto total) y
  `BaseDelay` mayor a cero.
- `FacturamaOptions.Validate` valida también que `Timeout` sea mayor a cero.
- Namespaces unificados bajo `FacturamaNetSDK.*` (se eliminaron restos de otros SDKs
  `Facturama.Sdk.Core.*` / `FacturamaAPI.src.*` y el typo `FacturamaNetSDk`).
- `ItemResponse` de CFDI unificado en `Models/Cfdi/Responses/Common/` (era duplicado idéntico
  entre CfdiWeb y CfdiLite).
- `RetentionEndpoint` alineado al patrón canónico (namespace file-scoped, `const Resource`,
  XML docs, validación de argumentos).
- `QueryBuilder` pasó a `internal`.
- El mapeo de códigos de estado en `FacturamaHttpClient` compara enteros en vez del enum
  `HttpStatusCode`: 422 y 429 no existen en el enum de `netstandard2.0` ni en el de .NET
  Framework. El caso redundante de 500 se absorbió en la rama `>= 500`.
- Proyectos `Tests` y `Sandbox` migrados a `net8.0`. ⚠️ **A definir con el equipo:** al correr
  en `net8.0`, las pruebas resuelven el asset `net8.0` del SDK, así que las ramas `#if` de
  `netstandard2.0` se compilan pero no se ejecutan en la suite. Cubrirlas de verdad exige
  añadir un TFM `net48` al proyecto de pruebas, lo que ata el CI a Windows.

### Corregido
- **Detección de timeout en net8.** `FacturamaHttpClient` solo miraba el primer nivel de
  `InnerException` buscando la `TimeoutException`; .NET 8 envuelve el timeout en una
  `TaskCanceledException` adicional y dejaba la señal un nivel más abajo. Un timeout que
  coincidía con un token ya cancelado se filtraba al consumidor como `TaskCanceledException`
  cruda en vez de `FacturamaTimeoutException`. Ahora se recorre toda la cadena.
- `TimeSpan` se multiplica vía ticks (`BackoffDelay` y el presupuesto total de la operación):
  el operador `*` de `TimeSpan` no existe en `netstandard2.0`. El redondeo es equivalente.
- `ClientEndpoint.ListAsync` ya no puede devolver `null` (devuelve lista vacía).
- Catálogos del SAT (`ProductService`, `CfdiType`, `Currency`, etc.) ahora deserializan todas sus
  propiedades (antes eran `get`-only y llegaban vacías).
- `FacturamaRateLimitException.RetryAfter` se completa desde el header `Retry-After`.
- `BrokenCircuitException` de Polly ahora se traduce a `FacturamaServerException` (503) en vez de
  filtrarse al consumidor.
- `ArgumentException` en `CatalogEndpoint` con `paramName` correcto.
- Typos en tipos/archivos: `IssuerResponse`, `SubscriptionPlanResponse`, `BranchOfficeResponse`,
  `TariffFractions`, `CfdiListResponse.cs`.

### Eliminado
- Enum `FacturamaEnvironment` duplicado en `Enums/` (código muerto).

## [1.0.0] — pendiente
- Primera versión estable (por definir).
