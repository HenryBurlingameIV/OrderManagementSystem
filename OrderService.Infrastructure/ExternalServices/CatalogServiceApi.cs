using Newtonsoft.Json.Linq;
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
    public class CatalogServiceApi : ICatalogServiceApi
    {
        private HttpClient _httpClient;
        public CatalogServiceApi(HttpClient httpClient) 
        {
            _httpClient = httpClient;
        }

        public async Task<ProductDto?> GetProductByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var url = $"api/products/{id}";
            var response = await _httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                await CreateAndThrowHttpRequestExceptionAsync(response, cancellationToken);
            }
            return await response.Content.ReadFromJsonAsync<ProductDto>(cancellationToken);
        }

        public async Task UpdateProductInventoryAsync(Guid id, int deltaQuantity, CancellationToken cancellationToken)
        {
            var response = await _httpClient.PatchAsJsonAsync(
                $"api/products/{id}/quantity", 
                new UpdateQuantityRequest(deltaQuantity), 
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                await CreateAndThrowHttpRequestExceptionAsync(response, cancellationToken);
            }
        }

        private async Task CreateAndThrowHttpRequestExceptionAsync(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            var errorJson = await response.Content!.ReadAsStringAsync(cancellationToken);
            var errorMessage = JObject.Parse(errorJson)["message"]?.ToString() ?? errorJson;
            throw new HttpRequestException(errorMessage, null, response.StatusCode);
        }
    }
}
