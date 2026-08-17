using FacturamaNetSDK.Models.Product.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FacturamaNetSDK.Models.Client.Response
{
    public sealed record FilterClientResponse
    {
        /// <summary>
        /// Total de productos existentes, sin aplicar el filtro de búsqueda.
        /// </summary>
        public int RecordsTotal { get; init; }

        /// <summary>
        /// Total de productos que coinciden con el filtro de búsqueda.
        /// </summary>
        public int RecordsFiltered { get; init; }

        /// <summary>
        /// Productos incluidos en la página solicitada.
        /// </summary>
        public IReadOnlyList<ClientResponse> Data { get; init; } = Array.Empty<ClientResponse>();
    }
}
