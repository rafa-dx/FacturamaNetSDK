# Changelog

Todos los cambios notables de este proyecto se documentan aquí.
El formato se basa en [Keep a Changelog](https://keepachangelog.com/es-ES/1.0.0/)
y el versionado sigue [SemVer](https://semver.org/lang/es/).

## [Sin publicar]

### Añadido
- Documentación XML (`GenerateDocumentationFile`) y metadatos de paquete NuGet en el `.csproj`.
- README con inicio rápido, manejo de errores y ejemplos reales.
- Este CHANGELOG.
- Comentarios XML en los modelos de request principales (CfdiRequest, Item, Receiver, Issuer).

### Cambiado
- Namespaces unificados bajo `FacturamaNetSDK.*` (se eliminaron restos de otros SDKs
  `Facturama.Sdk.Core.*` / `FacturamaAPI.src.*` y el typo `FacturamaNetSDk`).
- `ItemResponse` de CFDI unificado en `Models/Cfdi/Responses/Common/` (era duplicado idéntico
  entre CfdiWeb y CfdiLite).
- `RetentionEndpoint` alineado al patrón canónico (namespace file-scoped, `const Resource`,
  XML docs, validación de argumentos).
- `QueryBuilder` pasó a `internal`.

### Corregido
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
