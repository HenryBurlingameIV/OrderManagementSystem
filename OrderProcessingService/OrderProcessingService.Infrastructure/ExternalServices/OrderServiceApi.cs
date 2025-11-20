using Newtonsoft.Json.Linq;
using OrderProcessingService.Application.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace OrderProcessingService.Infrastructure.ExternalServices
{
    public class OrderServiceApi : IOrderServiceApi
    {
        private readonly HttpClient _httpClient;

        public OrderServiceApi(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task UpdateStatus(Guid id, string newStatus, CancellationToken cancellationToken)
        {
            var response = await _httpClient.PatchAsJsonAsync(
                $"api/orders/{id}/status",
                new { OrderStatus = newStatus },
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
