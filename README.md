# FacturamaNetSDK

SDK en .NET para consumir la API de facturación electrónica de [Facturama](https://facturama.mx) (CFDI 4.0, México).

Cubre CFDI (API Web y API Lite multiemisor), clientes, catálogos del SAT y retenciones, con resiliencia (reintentos + circuit breaker vía Polly), logging opcional y una jerarquía de excepciones tipadas.

> ⚠️ Proyecto en desarrollo. La superficie pública puede cambiar hasta la versión 1.0 estable.

---

## Requisitos

El paquete multiplataforma dos targets:

| Target | Cubre |
|--------|-------|
| `net8.0` | .NET 8 y superior |
| `netstandard2.0` | .NET Framework 4.6.1+, .NET Core 2.0+, .NET 5/6/7 |

También necesitas credenciales de Facturama (usuario y contraseña). Regístrate para el entorno sandbox en [Facturama](https://facturama.mx).

### Notas para consumidores en .NET Framework

En `netstandard2.0` el SDK trae `System.Text.Json` como paquete (en `net8.0` viene en el
framework compartido). Si tu proyecto es .NET Framework:

- Habilita `<AutoGenerateBindingRedirects>true</AutoGenerateBindingRedirects>`. Sin los
  redirects, `System.Text.Json` y sus dependencias (`System.Memory`, `System.Buffers`)
  fallan al cargar en tiempo de ejecución.
- Usa .NET Framework **4.7 o superior**, o habilita TLS 1.2 explícitamente
  (`ServicePointManager.SecurityProtocol`). La API de Facturama exige TLS 1.2 y las
  versiones anteriores no lo activan por defecto.
- Un timeout de red se reporta igual como `FacturamaTimeoutException`, pero el
  `CancellationToken` no interrumpe la lectura del cuerpo de la respuesta una vez iniciada:
  ahí el corte lo pone el timeout de la petición.

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

- **Reintentos:** 3 reintentos (4 intentos) con backoff exponencial **con jitter**, solo en
  errores transitorios —5xx, 408 y **429**— y solo en verbos idempotentes (GET, PUT, DELETE).
  POST no se reintenta por defecto.
- **`Retry-After`:** si la API envía la cabecera, el SDK la respeta en lugar del backoff
  calculado, acotada por `RetryOptions.MaxDelay` (10 s).
- **Circuit breaker:** abre tras **5 operaciones fallidas consecutivas** y se recupera a los 30 s.
  Es único por cliente, así que la protección aplica a la cuenta completa y no a cada ruta.

Con el circuito abierto, las peticiones fallan de inmediato con `FacturamaServerException` (503)
sin llegar a la red y sin consumir reintentos.

> Los umbrales cuentan **operaciones**, no intentos: una llamada que agota sus 4 intentos suma
> un solo fallo. Puedes ajustar el breaker y los reintentos por separado, sin relación entre ellos.

> Un **429 se reintenta pero no abre el circuito**: la API está sana, solo pide que bajes el
> ritmo. Como efecto colateral, un 429 reinicia la racha de fallos consecutivos del breaker.

### Presupuesto de la operación

`FacturamaOptions.Timeout` (10 s por defecto) es el techo de **un intento**, no de la llamada
completa. El SDK calcula el techo total y lo aplica a `HttpClient.Timeout`:

```
Total = Timeout × (MaxRetries + 1) + MaxDelay × MaxRetries + 5 s de margen
      = 10 × 4 + 10 × 3 + 5 = 75 s
```

Ese es el peor caso, no la latencia habitual: una API que responde 5xx al instante agota los
reintentos en ~14 s. Si tu aplicación es interactiva y 75 s es demasiado, baja `Timeout` y
`MaxRetries` — son las dos palancas que más pesan.

```csharp
var client = new FacturamaClient(options =>
{
    options.Username = "usuario";
    options.Password = "contraseña";
    options.CircuitBreaker = new CircuitBreakerOptions
    {
        FailuresBeforeBreaking = 5,
        BreakDuration = TimeSpan.FromSeconds(30)
    };
});
```

### Limitación conocida: degradación parcial

El breaker cuenta fallos **consecutivos**, así que un solo éxito intercalado reinicia el
contador. Si la API alterna éxitos y fallos —degradación parcial en vez de caída total— el
circuito no abre.

Cubrir ese caso requiere abrir por *proporción* de fallos sobre una ventana deslizante, lo que
exige un volumen sostenido (del orden de una operación cada 3 segundos) que un consumidor de
facturación típico no alcanza. Se dejó fuera de 1.0.0 y se evaluará para una versión posterior
si aparece el caso de uso de timbrado masivo concurrente.

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
