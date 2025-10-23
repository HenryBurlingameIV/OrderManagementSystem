using OrderProcessingService.Application.Contracts;
using OrderService.Api.Protos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderProcessingService.Infrastructure.ExternalServices
{
    public class OrderGrpcClient : IOrderServiceApi
    {
        private readonly Order.OrderClient _client;

        public OrderGrpcClient(Order.OrderClient client)
        {
            _client = client;
        }
        public async Task UpdateStatus(Guid id, string newStatus, CancellationToken cancellationToken)
        {
            var request = new UpdateOrderStatusRequest()
            {
                OrderId = id.ToString(),
                NewStatus = newStatus
            };

            await _client.UpdateOrderStatusAsync(request, cancellationToken: cancellationToken);
        }
    }
}
