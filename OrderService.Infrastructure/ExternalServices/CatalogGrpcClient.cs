using OrderService.Application.Contracts;
using OrderService.Application.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogService.Api.Protos;

namespace OrderService.Infrastructure.ExternalServices
{
    public class CatalogGrpcClient : ICatalogServiceApi
    {
        private readonly Catalog.CatalogClient _client;

        public CatalogGrpcClient(Catalog.CatalogClient client)
        {
            _client = client;
        }
        public async Task ReleaseProductAsync(Guid id, int quantity, CancellationToken cancellationToken)
        {
            var request = new ReleaseProductRequest()
            {
                ProductId = id.ToString(),
                Quantity = quantity,
            };


            await _client.ReleaseProductAsync(request, cancellationToken: cancellationToken);
        }

        public async Task<ProductDto?> ReserveProductAsync(Guid id, int quantity, CancellationToken cancellationToken)
        {
            var request = new ReserveProductRequest()
            {
                ProductId = id.ToString(),
                Quantity = quantity,
            };


            var response = await _client.ReserveProductAsync(request, cancellationToken: cancellationToken);

            return new ProductDto(
                Guid.Parse(response.ProductId),
                decimal.Parse(response.Price),
                quantity);
        }
    }
}
