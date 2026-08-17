# FacturamaNetSDK

SDK **no oficial** en .NET para consumir la API de facturación electrónica de [Facturama](https://facturama.mx) (CFDI 4.0, México).

Cubre CFDI (API Web y API Lite multiemisor), clientes, catálogos del SAT y retenciones, con resiliencia (reintentos + circuit breaker vía Polly), logging opcional y una jerarquía de excepciones tipadas.

> ⚠️ Proyecto en desarrollo. La superficie pública puede cambiar hasta la versión 1.0 estable.

---

## Requisitos

- .NET 6.0 o superior
- Credenciales de Facturama (usuario y contraseña). Regístrate para el entorno sandbox en [Facturama](https://facturama.mx).

## Instalación

```bash
dotnet add package FacturamaNetSDK
```

> Aún no publicado en NuGet. Mientras tanto, referencia el proyecto directamente o compílalo localmente.

---

## Inicio rápido

### 1. Crear el cliente

```csharp
using FacturamaNetSDK.Client;
using FacturamaNetSDK.Configuration;

// Sencillo — Sandbox por defecto
var client = new FacturamaClient("usuario", "contraseña");

// Con ambiente explícito
var client = new FacturamaClient("usuario", "contraseña", FacturamaEnvironment.Production);

// Avanzado — configuración completa (+ logger opcional)
var client = new FacturamaClient(options =>
{
    options.Username        = "usuario";
    options.Password        = "contraseña";
    options.Environment     = FacturamaEnvironment.Sandbox;
    options.Timeout         = TimeSpan.FromSeconds(60);
    options.ApiLiteVersion  = ApiLiteVersion.V4;
}, logger);
```

> **Credenciales:** nunca las escribas en el código ni las subas al repositorio. Léelas de variables
> de entorno o `dotnet user-secrets`:
>
> ```csharp
> options.Username = Environment.GetEnvironmentVariable("FACTURAMA_USER")!;
> options.Password = Environment.GetEnvironmentVariable("FACTURAMA_PASS")!;
> ```

### 2. Emitir un CFDI (API Web)

```csharp
using FacturamaNetSDK.Enums;
using FacturamaNetSDK.Models.Cfdi.Requests;

var request = new CfdiRequest
{
    NameId          = "1",
    Serie           = "FAC",
    Folio           = "99",
    CfdiType        = CfdiType.Ingreso.ToApiValue(),   // "I"
    PaymentForm     = "01",                            // catálogo SAT c_FormaPago
    PaymentMethod   = "PUE",                           // catálogo SAT c_MetodoPago
    ExpeditionPlace = "78000",                         // código postal del emisor
    Currency        = "MXN",
    Date            = DateTime.Now.ToString("s"),      // ISO 8601
    Receiver = new Receiver
    {
        Rfc          = "URE180429TM6",
        Name         = "UNIVERSIDAD ROBOTICA ESPAÑOLA",
        CfdiUse      = "G03",                           // catálogo SAT c_UsoCFDI
        FiscalRegime = "601",                          // catálogo SAT c_RegimenFiscal
        TaxZipCode   = "86991"
    },
    Items = new List<Item>
    {
        new Item
        {
            ProductCode = "10101504",                  // catálogo SAT c_ClaveProdServ
            UnitCode    = "MTS",                       // catálogo SAT c_ClaveUnidad
            Unit        = "NO APLICA",
            Description = "Estudios de laboratorio",
            Quantity    = 2.0m,
            UnitPrice   = 50.0m,
            Subtotal    = 100.0m,
            TaxObject   = "02",
            Total       = 116.00m,
            Taxes = new List<Tax>
            {
                new Tax { Name = "IVA", Rate = 0.16m, Base = 100.00m, Total = 16.0m, IsRetention = false }
            }
        }
    }
};

var cfdi = await client.Cfdi.CreateAsync(request);
Console.WriteLine(cfdi.Id);
```

### 3. Operaciones disponibles

```csharp
// CFDI (API Web)
await client.Cfdi.GetAsync(id);
await client.Cfdi.ListAsync(filtros);
await client.Cfdi.DownloadAsync(CfdiFileType.Pdf, InvoiceType.Issued, id);
await client.Cfdi.SendByEmailAsync(id, "correo@ejemplo.mx", InvoiceType.Issued);
await client.Cfdi.CancelAsync(id, InvoiceType.Issued, motive: "02");

// CFDI Lite (multiemisor), Clientes, Catálogos SAT y Retenciones
await client.CfdiLite.CreateAsync(...);
await client.Clients.ListAsync();
await client.Catalogs.GetCfdiUsesAsync();
await client.Retentions.CreateAsync(...);
```

---

## Manejo de errores

Todas las excepciones derivan de `FacturamaException`:

```csharp
using FacturamaNetSDK.Exceptions;

try
{
    var cfdi = await client.Cfdi.CreateAsync(request);
}
catch (FacturamaValidationException ex)      // 400/422 — datos inválidos
{
    foreach (var error in ex.Errors)
        Console.WriteLine($"{error.Key}: {string.Join(", ", error.Value)}");
}
catch (FacturamaAuthenticationException)     // 401
{ /* credenciales inválidas */ }
catch (FacturamaNotFoundException ex)        // 404
{ /* recurso no encontrado: ex.ResourceId */ }
catch (FacturamaRateLimitException ex)       // 429
{ /* reintentar tras ex.RetryAfter */ }
catch (FacturamaException ex)                // base (servidor, timeout, conexión…)
{ Console.WriteLine($"[{ex.StatusCode}] {ex.Message}"); }
```

---

## Resiliencia

El pipeline HTTP incluye por defecto (vía Polly), sin que tengas que configurar nada:

- **Reintentos:** 3 intentos con backoff exponencial, solo en errores transitorios y solo en
  verbos idempotentes (GET, PUT, DELETE). POST no se reintenta por defecto.
- **Circuit breaker en dos capas**, ambas compartidas por todos los endpoints del cliente y
  con 30 s de recuperación:
  - **Racha:** abre tras 10 fallos *consecutivos*. Protege al consumidor de bajo volumen y
    detecta una caída total de la API.
  - **Ratio:** abre cuando más del 50 % de las peticiones falla en una ventana de 60 s, siempre
    que haya al menos 20 peticiones en ella. Detecta degradación parcial bajo carga.

Con el circuito abierto, las peticiones fallan de inmediato con `FacturamaServerException` (503)
sin llegar a la red.

> Ambas capas cuentan **intentos**, no operaciones: cada reintento pasa por ellas. Si ajustas los
> umbrales, `FailuresBeforeBreaking` y `MinimumThroughput` deben superar `MaxRetries + 1`, o una
> sola petición fallida dejará el circuito abierto. El SDK lo valida al construir el cliente.

```csharp
var client = new FacturamaClient(options =>
{
    options.Username = "usuario";
    options.Password = "contraseña";
    options.CircuitBreaker = new CircuitBreakerOptions
    {
        FailuresBeforeBreaking = 10,
        FailureRatio = 0.5,
        SamplingDuration = TimeSpan.FromSeconds(60),
        MinimumThroughput = 20,
        BreakDuration = TimeSpan.FromSeconds(30)
    };
});
```

---

## Ambientes

| Ambiente | URL base |
|----------|----------|
| Sandbox (default) | `https://apisandbox.facturama.mx` |
| Producción | `https://api.facturama.mx` |

---

## Licencia

[MIT](LICENSE) © 2026 Rafael Dorantes.

Este SDK no está afiliado ni respaldado oficialmente por Facturama.
