

using FacturamaNetSDK.Client;
using FacturamaNetSDK.Exceptions;
using System.Security.Cryptography.X509Certificates;

namespace FacturamaNetSDK.Sandbox.WebApiExamples
{
    public class ProductExample
    {
        private readonly FacturamaClient _client;

        private string? _productId; // guardamos el ID para usarlo en los siguientes ejemplos

        public ProductExample(FacturamaClient client)
        {
            _client = client;
        }

        public async Task RunAsync()
        {
            Console.WriteLine("\n========================================");
            Console.WriteLine("  PRODUCT — API Web");
            Console.WriteLine("========================================\n");
            try
            {
                await CreateAsync();
                //await GetAsync();
                //await ListAsync();
                await PutAsync();
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
            finally
            {
                Console.WriteLine("\n--- Limpieza de recursos ---");
                await DeleteAsync();
            }
        }

        private async Task CreateAsync()
        {
            Console.WriteLine("--- Crear PRODUCT ---");
            var request = new Models.Product.Request.ProductRequest
            {
               Unit = "Pieza",
                UnitCode = "H87",
                IdentificationNumber = "1234567890",
                Name = "Producto de prueba",
                Description = "Descripción del producto de prueba",
                Price = 100.00m,
                CodeProdServ = "01010101",
                ObjetoImp = "02",
                Taxes = new List<Models.Common.Tax>
                {
                    new Models.Common.Tax
                    {
                        Name = "IVA",
                        Rate = 0.16m,
                        IsRetention = false,
                        IsFederalTax = true,

                    }
                }
            };
            var response = await _client.Products.CreateAsync(request);
            _productId = response.Id;
            Console.WriteLine($"Producto creado con ID: {_productId}");
        }

        private async Task GetAsync()
        {
            Console.WriteLine("--- Obtener PRODUCT ---");
            if (string.IsNullOrWhiteSpace(_productId))
            {
                Console.WriteLine("No hay un PRODUCT creado para obtener.");
                return;
            }
            var response = await _client.Products.GetAsync(_productId);
            Console.WriteLine($"Producto obtenido: {response.Name} (ID: {response.Id})");
        }

        private async Task ListAsync()
        {
            Console.WriteLine("--- Listar PRODUCT ---");
            var filters = new Models.Filters.QueryOptions
            {
                Start = 0,
                Length = 10,
                Search = ""
            };
            var response = await _client.Products.ListAsync(filters);
            Console.WriteLine($"Total de productos: {response.RecordsTotal} (filtrados: {response.RecordsFiltered})");
            foreach (var product in response.Data)
            {
                Console.WriteLine($"- {product.Name} (ID: {product.Id})");
            }
        }
        private async Task DeleteAsync()
        {
            Console.WriteLine("--- Eliminar PRODUCT ---");
            if (string.IsNullOrWhiteSpace(_productId))
            {
                Console.WriteLine("No hay un PRODUCT creado para eliminar.");
                return;
            }
            await _client.Products.DeleteAsync(_productId);
            Console.WriteLine($"Producto eliminado con ID: {_productId}");
        }

        private async Task PutAsync()
        {
            Console.WriteLine("--- Actualizar PRODUCT ---");
            if (string.IsNullOrWhiteSpace(_productId))
            {
                Console.WriteLine("No hay un PRODUCT creado para actualizar.");
                return;
            }
            var request = new Models.Product.Request.ProductRequest
            {
                Unit = "Pieza",
                UnitCode = "H87",
                IdentificationNumber = "1234567890",
                Name = "Producto de prueba modificado",
                Description = "Descripción del producto de prueba modificado",
                Price = 100.00m,
                CodeProdServ = "01010101",
                ObjetoImp = "02",
                Taxes = new List<Models.Common.Tax>
                {
                    new Models.Common.Tax
                    {
                        Name = "IVA",
                        Rate = 0.16m,
                        IsRetention = false,
                        IsFederalTax = true,

                    }
                }
            };
            var response = await _client.Products.UpdateAsync(_productId, request);
            Console.WriteLine($"Producto actualizado: {response.Name} (ID: {response.Id})");
        }
    }
}
