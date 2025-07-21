using Microsoft.Extensions.DependencyInjection;
using OrderManagementSystem.Shared.Contracts;
using OrderProcessingService.Application.Contracts;
using OrderProcessingService.Application.DTO;
using OrderProcessingService.Infrastructure.Messaging.Messages;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderProcessingService.Infrastructure.Messaging.Handlers
{
    public class OrderCreatedMessageHandler : IMessageHandler<OrderCreatedMessage>
    {
        private readonly IOrderProcessingInitializer _processingInitializer;

        public OrderCreatedMessageHandler(IOrderProcessingInitializer processingInitializer)
            => _processingInitializer = processingInitializer;
        public async Task HandleAsync(OrderCreatedMessage message, CancellationToken cancellationToken)
        {
            var order = MapToOrderDto(message);            
            await _processingInitializer.InitializeProcessingAsync(order, cancellationToken);
            Log.Information($"Processing of order with ID {order.Id} initialized.");
        }

        private OrderDto MapToOrderDto(OrderCreatedMessage message)
        {
            var items = message.Items.Select(i => new OrderItemDto(i.Id, i.Quantity)).ToList();
            return new OrderDto(
                message.Id,
                items,
                message.CreatedAtUtc,
                message.UpdatedAtUtc
                );
        }
    }
}
