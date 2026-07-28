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
}, logger);

// Descomenta el ejemplo que quieras ejecutar contra el sandbox:
await new CfdiExample(client).RunAsync();
await new CatalogExample(client).RunAsync();
await new CfdiLiteExample(client).RunAsync();
await new RetentionExample(client).RunAsync();
await new TaxEntityExample(client).RunAsync();
await new SubscriptionPlanExample(client).Run();

Log.CloseAndFlush();