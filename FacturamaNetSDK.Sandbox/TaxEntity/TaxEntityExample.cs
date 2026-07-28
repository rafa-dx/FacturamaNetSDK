using FacturamaNetSDK.Client;
using FacturamaNetSDK.Exceptions;
using FacturamaNetSDK.Models.Common;
using System.ComponentModel;
using System.Text.Json;


namespace FacturamaNetSDK.Sandbox.TaxEntity
{
    public class TaxEntityExample
    {
        private readonly FacturamaClient _client;

        public TaxEntityExample(FacturamaClient client)
        {
            _client = client;
        }

        public async Task RunAsync()
        {
            Console.WriteLine("\n========================================");
            Console.WriteLine("  TaxEntity");
            Console.WriteLine("========================================\n");
            try
            {
                await GetAsync();
                await PutInfoAsync();
                await PutLogoAsync();
                await PostBranchOfficeAsync();
                await GetBranchOfficeAsync();
                await ListBranchOfficesAsync();
                await listSerieAsync();
                await PostSerieAsync();
                await DeleteSerieAsync();
 
                //await 
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

        private async Task GetAsync()
        {
            Console.WriteLine("Obteniendo información del perfil fiscal...");
            var request = await _client.TaxEntity.GetAsync();
            Print("Perfil fiscal", request);
        }

        private async Task PutInfoAsync()
        {
            Console.WriteLine("Actualizando información del perfil fiscal...");
            var request = new FacturamaNetSDK.Models.TaxEntity.Request.TaxEntityRequest
            {

                FiscalRegime = "601",
                ComercialName = null,
                Rfc = "EKU9003173C9",
                TaxName = "ESCUELA KEMPER URGATE",
                Email = "soporte-api@ejemplo.mx",
                OptionalEmail = "soporte-api@ejemplo.mx",
                TaxAddress = new Models.Common.Address 
                {
                    Street = "Del mar",
                    ExteriorNumber = "44",
                    InteriorNumber = "",
                    Neighborhood = "Miramar",
                    ZipCode = "26015",
                    Locality = "Hermosillo",
                    Municipality = "Hermosillo",
                    State = "Sonora",
                    Country = "MEXICO"
                }
            };
            await _client.TaxEntity.UpdateInfoAsync(request);
            Print("Perfil fiscal", request);
        }

        private async Task PutLogoAsync()
        {
            Console.WriteLine("Actualizando logo del perfil fiscal...");
            var request = new Models.TaxEntity.Request.TaxEntityLogoRequest
            {
                Image = "https://cdn-icons-png.flaticon.com/512/5650/5650378.png",
                Type = "png"

            };
            await _client.TaxEntity.UpdateLogoAsync(request);
            Print("Logo", request);
        }

        private async Task PostBranchOfficeAsync()
        {
            var branchOfficeRequest = new Models.BranchOffice.Request.BranchOfficeRequest
            {
                Name = "Sucursal de prueba SDK net6",
                Description = "Test, agregado sucursal desde SDK net6",
                Address = new Address
                {
                    Street = "Calle de prueba",
                    ExteriorNumber = "123",
                    InteriorNumber = "A",
                    Neighborhood = "Colonia de prueba",
                    ZipCode = "78000",
                    Locality = "Localidad",
                    Municipality = "San Luis Potosi",
                    State = "San Luis Potosi",
                    Country = "México"
                }
            };

            var request = await _client.BranchOffices.AddAsync(branchOfficeRequest);
            Print("Sucursal agregada", request);
        }

        private async Task GetBranchOfficeAsync()
        {
            Console.WriteLine("Listando sucursales del perfil fiscal...");
            var request = await _client.BranchOffices.GetAsync("GHtxdqCdMm2qTLXvo8Eqgg2");
            Print("Sucursales", request);
        }
        private async Task ListBranchOfficesAsync()
        {
            Console.WriteLine("Listando sucursales del perfil fiscal...");
            var request = await _client.BranchOffices.ListAsync();
            Print("Sucursales", request);
        }

        private async Task listSerieAsync()
        {
            Console.WriteLine("Listando series del perfil fiscal...");
            var request = await _client.Series.ListAsync("GHtxdqCdMm2qTLXvo8Eqgg2");
            Print("Series", request);
        }

        private async Task PostSerieAsync()
        {
            Console.WriteLine("Agregando serie del perfil fiscal...");
            var serieRequest = new Models.Series.Request.SerieRequest
            {
                IdBranchOffice = "GHtxdqCdMm2qTLXvo8Eqgg2",
                Name = "SDKNET76",
                Description = "Serie agregada desde SDK net6",
                Folio = 1
            };
            var request = await _client.Series.AddAsync("GHtxdqCdMm2qTLXvo8Eqgg2", serieRequest);
            Print("Serie agregada", request);
        }

        private async Task DeleteSerieAsync()
        {
            Console.WriteLine("Eliminando serie del perfil fiscal...");
            var request = await _client.Series.DeleteAsync("GHtxdqCdMm2qTLXvo8Eqgg2", "SDKNET6");
            Print("Serie eliminada", request);
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
