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
    .WriteTo.Console(outputTemplate:
        "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

var loggerFactory = new SerilogLoggerFactory(Log.Logger);
var logger = loggerFactory.CreateLogger("FacturamaSDK");

// Pasar logger al cliente
var client = new FacturamaClient(options =>
{
    options.Environment = FacturamaEnvironment.Sandbox;
    options.Username = EnvironmentConfiguration.Username;
    options.Password = EnvironmentConfiguration.Password;
    //options.ApiLiteVersion = ApiLiteVersion.V3;
    options.Retry = new RetryOptions
    {
        Enabled = true,
        MaxRetries = 5,
        BaseDelay = TimeSpan.FromSeconds(2),
        RetryPost = false,
    };
    // Ambas capas cuentan intentos, no operaciones: con MaxRetries = 2 cada operación consume
    // hasta 3. Los umbrales toleran 2 operaciones completas fallidas antes de abrir.
    options.CircuitBreaker = new CircuitBreakerOptions
    {
        Enabled = false,
        // Capa 1 — racha de fallos consecutivos (cubre el volumen bajo).
        //FailuresBeforeBreaking = 7,
        // Capa 2 — proporción de fallos en ventana deslizante (cubre la degradación parcial).
        //FailureRatio = 0.5,
        //SamplingDuration = TimeSpan.FromSeconds(60),
        //MinimumThroughput = 20,
        //BreakDuration = TimeSpan.FromSeconds(30),
    };

}, logger);

// Descomenta el ejemplo que quieras ejecutar contra el sandbox:
//await new CfdiExample(client).RunAsync();
await new ProductExample(client).RunAsync();
//await new CatalogExample(client).RunAsync();
//await new CfdiLiteExample(client).RunAsync();
//await new RetentionExample(client).RunAsync();
//await new TaxEntityExample(client).RunAsync();
//await new SubscriptionPlanExample(client).RunAsync();

Log.CloseAndFlush();
client.Dispose();