
using FacturamaNetSDK.Enums;
using FacturamaNetSDK.Client;
using FacturamaNetSDK.Exceptions;
using FacturamaNetSDK.Models.Cfdi.Requests;
using FacturamaNetSDK.Models.Common;
using FacturamaNetSDK.Models.Filters;
using FacturamaNetSDK.Utilities;
using System.Text.Json;

namespace FacturamaNetSDK.Sandbox.WebApiExamples;

public class CfdiExample
{
    private readonly FacturamaClient _client;
    private string? _cfdiId; // guardamos el ID para usarlo en los siguientes ejemplos

    public CfdiExample(FacturamaClient client)
    {
        _client = client;
    }

    public async Task RunAsync()
    {
        Console.WriteLine("\n========================================");
        Console.WriteLine("  CFDI — API Web");
        Console.WriteLine("========================================\n");

        try
        {
            await CreateAsync();
            await GetAsync();
            await ListAsync();
            //await GetStatusAsync();
            await DownloadPdfAsync();
            await DownloadXmlAsync();
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

    // -------------------------------------------------------------------------

    private async Task CreateAsync()
    {
        Console.WriteLine("--- Crear CFDI ---");

        var request = new CfdiRequest
        {
            NameId = "1",
            Folio = "99",
            Serie = "FAC",
            CfdiType = CfdiType.Ingreso.ToApiValue(),
            PaymentForm = "01",
            PaymentMethod = "PUE",
            ExpeditionPlace = "78000",
            OrderNumber = "TEST-001",
            Currency = "MXN",
            Date = DateTime.Now.ToString("s"),
            PaymentConditions = "CREDITO A SIETE DIAS",
            Observations = "Solo visible en PDF",
            Receiver = new Receiver
            {
                Rfc = "URE180429TM6",
                Name = "UNIVERSIDAD ROBOTICA ESPAÑOLA",
                CfdiUse = "G03",
                FiscalRegime = "601",
                TaxZipCode = "86991"
            },
            Items = new List<Item>
            {
                new Item
                {
                    ProductCode = "10101504",
                    UnitCode = "MTS",
                    Unit = "NO APLICA",
                    Description = "Estudios de laboratorio",
                    Quantity = 2.0m,
                    UnitPrice = 50.0m,
                    Subtotal = 100.0m,
                    Discount = 0.0m,
                    TaxObject = "02",
                    Total = 116.00m,
                    Taxes = new List<Tax>
                    {
                        new Tax
                        {
                            Name = "IVA",
                            Rate = 0.16m,
                            Total = 16.0m,
                            Base = 100.00m,
                            IsRetention = false
                        }
                    }
                }
            }


        };

        var response = await _client.Cfdi.CreateAsync(request);
        _cfdiId = response.Id;

        Print("CFDI creado", response);
    }

    private async Task GetAsync()
    {
        if (_cfdiId is null) { Console.WriteLine("--- Get: sin ID, omitido ---\n"); return; }
        Console.WriteLine("--- Obtener CFDI ---");

        var response = await _client.Cfdi.GetAsync(_cfdiId);
        Print("CFDI obtenido", response);
    }

    private async Task ListAsync()
    {
        Console.WriteLine("--- Listar CFDIs ---");
        var filters = new CfdiFilter
        {
            Type = "issued",
            FolioStart = -1,
            FolioEnd = -1,
            Rfc = null,
            DateStart = null,
            DateEnd = null,
            Status = "all",
            OrderNumber = null,
            TaxEntityName = null,
            IdBranch = null,
            Serie = null,
            Id = null,
            InvoiceType = null,
            PaymentMethod = null,
            RfcIssuer = null,
            Page = 0,
            keyword = null,
        };

        var response = await _client.Cfdi.ListAsync(filters);
        Console.WriteLine($"Total CFDIs: {response?.Count ?? 0}");

        foreach (var cfdi in response)
            Console.WriteLine($"  ID: {cfdi.Id} | Folio: {cfdi.Folio} | Total: {cfdi.Total:C}");

        Console.WriteLine();
    }

    private async Task GetStatusAsync()
    {
        Console.WriteLine("--- Status SAT ---");

        var statusFilter = new CfdiStatusParams
        {
            Uuid = "27568D31-7E57-442F-BA77-798CBF30BD7D",
            ReceiverRfc = "URE180429TM6",
            IssuerRfc = "AAA010101AAA",
            Total = "116.00"

        };

        var response = await _client.Cfdi.GetStatusAsync(statusFilter);

        Console.WriteLine($"  Status: {response.Status}");
        Console.WriteLine($"  IsCancelable: {response.IsCancelable}");
        Console.WriteLine($"  UUID: {response.Uuid}\n");
    }

    private async Task DownloadPdfAsync()
    {
        if (_cfdiId is null) { Console.WriteLine("--- Download PDF: sin ID, omitido ---\n"); return; }
        Console.WriteLine("--- Descargar PDF ---");

        var response = await _client.Cfdi.DownloadAsync(CfdiFileType.Pdf, InvoiceType.Issued, _cfdiId);
        Console.WriteLine($"  ContentType: {response.ContentType}");
        Console.WriteLine($"  Tamaño: {response.ContentLength} bytes\n");
        Console.WriteLine($"  Base64 (primeros 100 chars): {response.ContentBase64}");
    }

    private async Task DownloadXmlAsync()
    {
        if (_cfdiId is null) { Console.WriteLine("--- Download XML: sin ID, omitido ---\n"); return; }
        Console.WriteLine("--- Descargar XML ---");

        var response = await _client.Cfdi.DownloadAsync(CfdiFileType.Xml, InvoiceType.Issued, _cfdiId);
        Console.WriteLine($"  ContentType: {response.ContentType}");
        Console.WriteLine($"  Tamaño: {response.ContentLength} bytes\n");
    }

    private async Task SendByEmailAsync()
    {
        if (_cfdiId is null) { Console.WriteLine("--- SendByEmail: sin ID, omitido ---\n"); return; }
        Console.WriteLine("--- Enviar por Email ---");

        var response = await _client.Cfdi.SendByEmailAsync(_cfdiId, "prueba@facturama.mx", InvoiceType.Issued);
        Console.WriteLine($"  Success: {response.Success}");
        Console.WriteLine($"  Mensaje: {response.Msj}\n");
    }

    private async Task CancelAsync()
    {
        if (_cfdiId is null) { Console.WriteLine("--- Cancel: sin ID, omitido ---\n"); return; }
        Console.WriteLine("--- Cancelar CFDI ---");

        var response = await _client.Cfdi.CancelAsync(_cfdiId, InvoiceType.Issued, motive: "02");
        Console.WriteLine($"  Status: {response.Status}");
        Console.WriteLine($"  Mensaje: {response.Message}\n");
    }

    // -------------------------------------------------------------------------

    private static void Print<T>(string label, T obj)
    {
        var json = JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true });
        Console.WriteLine($"[{label}]");
        Console.WriteLine(json);
        Console.WriteLine();
    }
}