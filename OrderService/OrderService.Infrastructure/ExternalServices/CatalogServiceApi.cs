using Newtonsoft.Json.Linq;
using OrderService.Application.Contracts;
using OrderService.Application.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OrderService.Infrastructure.ExternalServices
{
    public class CatalogServiceApi : ICatalogServiceApi
    {
        private HttpClient _httpClient;
        public CatalogServiceApi(HttpClient httpClient) 
        {
            _httpClient = httpClient;
        }


        public async Task<ProductDto?> ReserveProductAsync(Guid id, int quantity, CancellationToken cancellationToken)
        {

            var response = await _httpClient.PatchAsJsonAsync(
               $"api/products/{id}/reserve",
               new { Quantity = quantity },
               cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                await CreateAndThrowHttpRequestExceptionAsync(response, cancellationToken);
            }
            return await response.Content.ReadFromJsonAsync<ProductDto>(cancellationToken);
        }

        public async Task ReleaseProductAsync(Guid id, int quantity, CancellationToken cancellationToken)
        {
            var response = await _httpClient.PatchAsJsonAsync(
                $"api/products/{id}/release",
                new { Quantity = quantity },
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
