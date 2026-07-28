
using FacturamaNetSDK.Client;
using FacturamaNetSDK.Exceptions;
using FacturamaNetSDK.Models.Common.Catalogs;
using System.Text.Json;

namespace FacturamaNetSDK.Sandbox.WebApiExamples
{
    public class CatalogExample
    {

        private readonly FacturamaClient _client;

        public CatalogExample(FacturamaClient client)
        {
            _client = client;
        }

        public async Task RunAsync()
        {
            Console.WriteLine("\n========================================");
            Console.WriteLine("  Catálogos — API Web");
            Console.WriteLine("========================================\n");
            try
            {
                await GetPostalCodes();
                await GetCountries();
                await GetStates();
                await GetMunicipalities();
                await GetLocalities();
                await GetNeighborhoods();
                await GetCfdiUse();
                await GetUnits();
                await GetProductsOrServices();
                await GetNameIds();
                await GetCurrencies();
                await GetBanks();
                await GetPaymentMethods();
                await GetPaymentForms();
                await GetFiscalRegimens();
                await GetCfdiTypes();
                await GetTariffFractions();
                await GetIncoterm();
                await GetClaveUnidadPeso();
                await GetCatalogTransportKey();
                await GetCondicionesEspeciales();
                await GetConfigAutotransporte();
                await GetDocumentoAduanero();
                await GetFormaFarmaceutica();
                await GetTipoEmbalaje();
                await GetSubTipoRemolque();
                await GetMaterialPeligroso();
                await GetTipoMateria();
                await GetTipoPermiso();
                await GetRegimenAduaneroEntrada();
                await GetRegimenAduaneroSalida();
                await GetRegistroISTMO();
                await GetSectorCOFEPRIS();
                await GetSubTipoRemolque();
                await GetMercancias();

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

        private async Task GetPostalCodes()
        {
            Console.WriteLine("--- Obtener código postal ---");
            var PostalCode = await _client.Catalogs.GetPostalCodesAsync("78140");
            Print("Codigos Postales", PostalCode);

        }

        private async Task GetCountries()
        {
            Console.WriteLine("--- Obtener países ---");
            var countries = await _client.Catalogs.GetCountriesAsync();
            Print("Países", countries);

        }

        private async Task GetStates()
        {
            Console.WriteLine("--- Obtener estados ---");
            var states = await _client.Catalogs.GetStatesAsync("MEX");
            Print("Estados", states);
        }

        private async Task GetMunicipalities()
        {
            Console.WriteLine("--- Obtener municipios ---");
            var municipalities = await _client.Catalogs.GetMunicipalitiesAsync("SLP");
            Print("Municipios", municipalities);
        }


        private async Task GetLocalities()
        {
            Console.WriteLine("--- Obtener localidades ---");
            var localities = await _client.Catalogs.GetLocalitiesAsync("MEX");
            Print("Localidades", localities);
        }

        private async Task GetNeighborhoods()
        {
            Console.WriteLine("--- Obtener colonias ---");
            var neighborhoods = await _client.Catalogs.GetNeighborhoodsAsync("78140");
            Print("Colonias", neighborhoods);
        }

        private async Task GetCfdiUse()
        {
            Console.WriteLine("--- Obtener usos de CFDI ---");
            var cfdiUse = await _client.Catalogs.GetCfdiUsesAsync();
            Print("Usos de CFDI", cfdiUse);
        }

        private async Task GetUnits()
        {
            Console.WriteLine("--- Obtener unidades de medida ---");
            var units = await _client.Catalogs.GetUnitsAsync("18");
            Print("Unidades de medida", units);
        }

        private async Task GetProductsOrServices()
        {
            Console.WriteLine("--- Obtener productos o servicios ---");
            var productsOrServices = await _client.Catalogs.GetProductsServicesAsync("desarrollo");
            Print("Productos o servicios", productsOrServices);
        }

        private async Task GetNameIds()
        {
            Console.WriteLine("--- Obtener NameIDs ---");
            var nameIds = await _client.Catalogs.GetNameIdsAsync();
            Print("NameIDs", nameIds);
        }

        private async Task GetCurrencies()
        {
            Console.WriteLine("--- Obtener monedas ---");
            var currencies = await _client.Catalogs.GetCurrenciesAsync("mexicano");
            Print("Monedas", currencies);
        }

        private async Task GetBanks()
        {
            Console.WriteLine("--- Obtener bancos ---");
            var banks = await _client.Catalogs.GetBanksAsync();
            Print("Bancos", banks);
        }

        private async Task GetPaymentMethods()
        {
            Console.WriteLine("--- Obtener métodos de pago ---");
            var paymentMethods = await _client.Catalogs.GetPaymentMethodsAsync();
            Print("Métodos de pago", paymentMethods);
        }

        private async Task GetPaymentForms()
        {
            Console.WriteLine("--- Obtener formas de pago ---");
            var paymentForms = await _client.Catalogs.GetPaymentFormsAsync();
            Print("Formas de pago", paymentForms);
        }

        private async Task GetFiscalRegimens()
        {
            Console.WriteLine("--- Obtener regímenes fiscales ---");
            var fiscalRegimens = await _client.Catalogs.GetFiscalRegimensAsync("EKU9003173C9");
            Print("Regímenes fiscales", fiscalRegimens);
        }

        private async Task GetCfdiTypes()
        {
            Console.WriteLine("--- Obtener tipos de CFDI ---");
            var cfdiTypes = await _client.Catalogs.GetCfdiTypesAsync();
            Print("Tipos de CFDI", cfdiTypes);
        }

        private async Task GetTariffFractions()
        {
            Console.WriteLine("--- Obtener fracciones arancelarias ---");
            var tariffFractions = await _client.Catalogs.GetTariffFractionsAsync("1234");
            Print("Fracciones arancelarias", tariffFractions);
        }

        private async Task GetIncoterm()
        {
            Console.WriteLine("--- Obtener Incoterms ---");
            var incoterm = await _client.Catalogs.GetIncotermAsync();
            Print("Incoterms", incoterm);
        }

       private async Task GetClaveUnidadPeso()
        {
            Console.WriteLine("--- Obtener clave unidad peso ---");
            var claveUnidadPeso = await _client.Catalogs.GetClaveUnidadPesoAsync();
            Print("Clave unidad peso", claveUnidadPeso);
        }

        private async Task GetCatalogTransportKey()
        {
            Console.WriteLine("--- Obtener claves de transporte ---");
            var transportKeys = await _client.Catalogs.GetCatalogTransportKeyAsync();
            Print("Claves de transporte", transportKeys);
        }

        private async Task GetCondicionesEspeciales()
        {
            Console.WriteLine("--- Obtener condiciones especiales ---");
            var condicionesEspeciales = await _client.Catalogs.GetCondicionesEspecialesAsync();
            Print("Condiciones especiales", condicionesEspeciales);
        }

        private async Task GetConfigAutotransporte()
        {
            Console.WriteLine("--- Obtener configuración de autotransporte ---");
            var configAutotransporte = await _client.Catalogs.GetConfigAutotransporteAsync();
            Print("Configuración de autotransporte", configAutotransporte);
        }

        private async Task GetDocumentoAduanero()
        {
            Console.WriteLine("--- Obtener documentos aduaneros ---");
            var documentoAduanero = await _client.Catalogs.GetDocumentoAduaneroAsync();
            Print("Documentos aduaneros", documentoAduanero);
        }

        private async Task GetFormaFarmaceutica()
        {
            Console.WriteLine("--- Obtener formas farmacéuticas ---");
            var formaFarmaceutica = await _client.Catalogs.GetFormaFarmaceuticaAsync();
            Print("Formas farmacéuticas", formaFarmaceutica);
        }

        private async Task GetTipoEmbalaje()
        {
            Console.WriteLine("--- Obtener tipos de embalaje ---");
            var tipoEmbalaje = await _client.Catalogs.GetTipoEmbalajeAsync();
            Print("Tipos de embalaje", tipoEmbalaje);
        }

        private async Task GetSubTipoRemolque()
        {
            Console.WriteLine("--- Obtener subtipos de remolque ---");
            var subTipoRemolque = await _client.Catalogs.GetSubTipoRemolqueAsync();
            Print("Subtipos de remolque", subTipoRemolque);
        }

        private async Task GetMaterialPeligroso()
        {
            Console.WriteLine("--- Obtener materiales peligrosos ---");
            var materialPeligroso = await _client.Catalogs.GetMaterialPeligrosoAsync("2111");
            Print("Materiales peligrosos", materialPeligroso);
        }

        private async Task GetTipoMateria()
        {
            Console.WriteLine("--- Obtener tipos de materia ---");
            var tipoMateria = await _client.Catalogs.GetTipoMateriaAsync();
            Print("Tipos de materia", tipoMateria);
        }
        private async Task GetTipoPermiso()
        {
            Console.WriteLine("--- Obtener tipos de permiso ---");
            var tipoPermiso = await _client.Catalogs.GetTipoPermisoAsync();
            Print("Tipos de permiso", tipoPermiso);
        }
        private async Task GetRegimenAduaneroEntrada()
        {
            Console.WriteLine("--- Obtener regímenes aduaneros ---");
            var regimenAduanero = await _client.Catalogs.GetRegimenAduaneroEntradaAsync();
            Print("Regímenes aduaneros", regimenAduanero);
        }
        private async Task GetRegimenAduaneroSalida()
        {
            Console.WriteLine("--- Obtener regímenes aduaneros ---");
            var regimenAduanero = await _client.Catalogs.GetRegimenAduaneroSalidaAsync();
            Print("Regímenes aduaneros", regimenAduanero);
        }
        private async Task GetRegistroISTMO()
        {
            Console.WriteLine("--- Obtener registros ISTMO ---");
            var registroISTMO = await _client.Catalogs.GetRegistroISTMOAsync();
            Print("Registros ISTMO", registroISTMO);
        }
        private async Task GetSectorCOFEPRIS()
        {
            Console.WriteLine("--- Obtener sectores COFEPRIS ---");
            var sectorCOFEPRIS = await _client.Catalogs.GetSectorCOFEPRISAsync();
            Print("Sectores COFEPRIS", sectorCOFEPRIS);
        }
        private async Task GetMercancias()
        {
            Console.WriteLine("--- Obtener mercancías ---");
            var mercancias = await _client.Catalogs.GetMercanciasAsync();
            Print("Mercancías", mercancias);
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
