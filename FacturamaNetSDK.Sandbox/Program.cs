using FacturamaNetSDK.Client;
using FacturamaNetSDK.Configuration;
using FacturamaNetSDK.Sandbox.APiLiteExamples;
using FacturamaNetSDK.Sandbox.RetentionExample;
using FacturamaNetSDK.Sandbox.WebApiExamples;
using Serilog;
using Serilog.Extensions.Logging;

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
    options.Username = "tu_usuario";
    options.Password = "tu_contraseña";
    //options.ApiLiteVersion = ApiLiteVersion.V3;
}, logger);

//await new CfdiExample(client).RunAsync();
//await new CatalogExample(client).RunAsync();
//await new CfdiLiteExample(client).RunAsync();
//await new RetentionExample(client).RunAsync();

Log.CloseAndFlush();