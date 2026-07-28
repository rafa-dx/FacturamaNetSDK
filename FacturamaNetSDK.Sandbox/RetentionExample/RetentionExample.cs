using FacturamaNetSDK.Models.Filters;
using FacturamaNetSDK.Enums;
using FacturamaNetSDK.Client;
using FacturamaNetSDK.Exceptions;
using FacturamaNetSDK.Models.Retentions.Request;
using System.Net.WebSockets;
using System.Text.Json;

namespace FacturamaNetSDK.Sandbox.RetentionExample
{
    public class RetentionExample
    {
        private readonly FacturamaClient _client;
        private string? _retentionId; // guardamos el ID para usarlo en los siguientes ejemplos

        public RetentionExample(FacturamaClient client)
        {
            _client = client;
        }

        public async Task RunAsync()
        {
            Console.WriteLine("\n========================================");
            Console.WriteLine("  Retención — API Web");
            Console.WriteLine("========================================\n");
            try
            {
                await CreateAsync();
                await GetAsync();
                await ListAsync();
                await DownloadPdfAsync();
                await SendByEmailAsync();
                await CancelAsync();
            }
            catch (FacturamaValidationException ex)
            {
                Console.WriteLine($"\n[Validación] {ex.Message}");
                foreach (var error in ex.Errors)
                    Console.WriteLine($"  {error.Key}: {string.Join(", ", error.Value)}");
            }
            catch (FacturamaAuthenticationException ex)
            {
                Console.WriteLine($"\n[Auth] {ex.Message}");
            }
            catch (FacturamaNotFoundException ex)
            {
                Console.WriteLine($"\n[NotFound] {ex.Message} — ResourceId: {ex.ResourceId}");
            }
            catch (FacturamaException ex)
            {
                Console.WriteLine($"\n[Error {ex.StatusCode}] {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n[Error inesperado] {ex.Message}");
            }
        }

        public async Task CreateAsync()
        {
            Console.WriteLine("Creando retención...");

            var CfdiRetention = new RetentionRequest
            {
                FolioInt = "12345",
                FechaExp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"),
                CveRetenc = "01",
                LugarExpRetenc = "78000",
                Emisor = new EmisorRequest
                {
                    RFCEmisor = "EKU9003173C9",
                    NomDenRazSocE = "ESCUELA KEMPER URGATE",
                    RegimenFiscalE = "601"
                },
                Receptor = new ReceptorRequest
                {
                    Nacionalidad = "Nacional",
                    Nacional = new NacionalRequest
                    {
                        RFCRecep = "CACX7605101P8",
                        NomDenRazSocR = "XOCHILT CASAS CHAVEZ",
                        DomicilioFiscalR = "36257"
                    }
                },
                Periodo = new PeriodoRequest
                {
                    MesIni = "01",
                    MesFin = "01",
                    Ejerc = "2023"
                },
                Totales = new TotalesRequest
                {
                    montoTotOperacion = 1681.06M,
                    montoTotGrav = 1681.06M,
                    montoTotExent = 0M,
                    montoTotRet = 151.29M,
                    ImpRetenidos = new List<ImpRetenido>
                    {
                        new ImpRetenido
                        {
                            BaseRet = 1681.06M,
                            Impuesto = "01",
                            MontoRet = 16.81m,
                            TipoPagoRet = "04"
                        },
                         new ImpRetenido
                        {
                            BaseRet = 268.96M,
                            Impuesto = "02",
                            MontoRet = 134.48m,
                            TipoPagoRet = "01"
                        }
                    }
                }
            };

            var retention = await _client.Retentions.CreateAsync(CfdiRetention);
            _retentionId = retention.Id; // guardamos el ID para usarlo en los siguientes ejemplos
            Print("Retención creada", retention);
        }

        public async Task GetAsync()
        {
            if (string.IsNullOrEmpty(_retentionId))
            {
                Console.WriteLine("No se ha creado una retención aún.");
                return;
            }
            Console.WriteLine($"Obteniendo retención con ID: {_retentionId}...");
            var retention = await _client.Retentions.GetAsync(_retentionId);
            Print("Retención obtenida", retention);
        }

        public async Task ListAsync()
        {
            Console.WriteLine("Listando retenciones...");
            var filter = new RetentionFilter
            {
                DateStart = "2026-02-01T12:00:01",
                DateEnd = "2026-03-28T12:00:01",
                Page = 1,
            };
            var retentions = await _client.Retentions.ListAsync(filter);
            Print("Retenciones listadas", retentions);
        }   

        public async Task CancelAsync()
        {
            Console.WriteLine("Cancelando retención...");

            if (string.IsNullOrEmpty(_retentionId))
            {
                Console.WriteLine("No se ha creado una retención aún.");
                return;
            }
            var cancellationResponse = await _client.Retentions.CancelAsync(_retentionId, motive: "02");
            Print("Retención cancelada", cancellationResponse);
        }


        public async Task SendByEmailAsync()
        {
            Console.WriteLine("Enviando retención por email...");
            if (string.IsNullOrEmpty(_retentionId))
            {
                Console.WriteLine("No se ha creado una retención aún.");
                return;
            }
            var email = "rafael@facturama.mx";
            var sendResponse = await _client.Retentions.SendByEmailAsync(_retentionId, email);
            Print("Retención enviada por email", sendResponse); 
        }

        public async Task DownloadPdfAsync()
        { 
         Console.WriteLine("Descargando retención en PDF...");
            if (string.IsNullOrEmpty(_retentionId))
            {
                Console.WriteLine("No se ha creado una retención aún.");
                return;
            }
            var pdfBytes = await _client.Retentions.DownloadAsync("pdf", _retentionId);
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), $"Retention_{_retentionId}.pdf");

            Print("Retención descargada en PDF", pdfBytes);
        }

        private static void Print<T>(string label, T obj)
        {
            var json = JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true });
            Console.WriteLine($"[{label}]");
            Console.WriteLine(json);
            Console.WriteLine();

        }
    }
}
