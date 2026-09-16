using FacturamaNetSDK.Client;
using FacturamaNetSDK.Configuration;
using FacturamaNetSDK.Sandbox.APiLiteExamples;
using FacturamaNetSDK.Sandbox.RetentionExample;
using FacturamaNetSDK.Sandbox.WebApiExamples;
using FacturamaNetSDK.Sandbox.Configuration;
using Serilog;
using Serilog.Extensions.Logging;
using FacturamaNetSDK.Sandbox.TaxEntity;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console(outputTemplate:"[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

var loggerFactory = new SerilogLoggerFactory(Log.Logger);
var logger = loggerFactory.CreateLogger("FacturamaSDK");

// Forma corta, equivalente a la configuración de abajo sin las opciones de resiliencia:
//var client = new FacturamaClient(EnvironmentConfiguration.Username, EnvironmentConfiguration.Password, FacturamaEnvironment.Sandbox);

// Pasar logger al cliente
var client = new FacturamaClient(options =>
{
    options.Environment = FacturamaEnvironment.Sandbox;
    // Para probar contra un mock local (WireMock, contenedor). Gana sobre Environment:
    //options.BaseUrlOverride = new Uri("http://localhost:7002/");
    options.Username = EnvironmentConfiguration.Username;
    options.Password = EnvironmentConfiguration.Password;
    //options.ApiLiteVersion = ApiLiteVersion.V3;
    options.Timeout = TimeSpan.FromSeconds(30);
    options.Retry = new RetryOptions
    {
        Enabled = true,
        MaxRetries = 5,
        BaseDelay = TimeSpan.FromSeconds(.5),
        RetryPost = false,
        MaxDelay = TimeSpan.FromSeconds(60),
    };
    // Los umbrales cuentan operaciones completas, no intentos: una llamada que agota sus
    // reintentos suma 1.
    options.CircuitBreaker = new CircuitBreakerOptions
    {
        Enabled = true,
        FailuresBeforeBreaking = 2,
        BreakDuration = TimeSpan.FromSeconds(30),
    };

}, logger);

// Descomenta el ejemplo que quieras ejecutar contra el sandbox:
await new CfdiExample(client).RunAsync();
//await new ProductExample(client).RunAsync();
//await new CatalogExample(client).RunAsync();
//await new CfdiLiteExample(client).RunAsync();
//await new RetentionExample(client).RunAsync();
//await new TaxEntityExample(client).RunAsync();
//await new SubscriptionPlanExample(client).RunAsync();

Log.CloseAndFlush();
client.Dispose();
