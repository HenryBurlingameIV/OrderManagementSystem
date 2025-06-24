using OrderService.Infrastructure.Contracts;
using OrderService.Infrastructure.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OrderService.Infrastructure.HttpClients
{
    public class CatalogServiceClient : ICatalogServiceClient
    {
        private IHttpClientFactory _httpClientFactory;
        public CatalogServiceClient(IHttpClientFactory factory) 
        {
            _httpClientFactory = factory;
        }

        public async Task<ProductDto?> GetProductByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var client = _httpClientFactory.CreateClient("catalog");
            //var product = await client.GetFromJsonAsync<ProductDto>($"/api/products/{id}", cancellationToken);

            var url = $"/api/products/{id}";

            var response = await client.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(errorMessage, null, response.StatusCode);
            }
            return await response.Content.ReadFromJsonAsync<ProductDto>(cancellationToken);
        }

        public async Task ReserveProductAsync(Guid id, int quantity, CancellationToken cancellationToken)
        {
            var client = _httpClientFactory.CreateClient("catalog");

            var response = await client.PatchAsJsonAsync(
                $"api/products/{id}/quantity", 
                new { Qiuantity = quantity }, 
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(errorMessage, null, response.StatusCode);
            }
        }
    }
}
