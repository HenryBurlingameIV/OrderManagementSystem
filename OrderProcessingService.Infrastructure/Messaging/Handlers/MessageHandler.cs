using Microsoft.Extensions.DependencyInjection;
using OrderManagementSystem.Shared.Contracts;
using OrderProcessingService.Application.Contracts;
using OrderProcessingService.Application.DTO;
using OrderProcessingService.Infrastructure.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderProcessingService.Infrastructure.Messaging.Handlers
{
    public class MessageHandler : IMessageHandler<OrderCreatedMessage>
    {
        private readonly IServiceProvider _serviceProvider;

        public MessageHandler(IServiceProvider serviceProvider)
            => _serviceProvider = serviceProvider;
        public async Task HandleAsync(OrderCreatedMessage message, CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var initializer = scope.ServiceProvider.GetRequiredService<IOrderProcessingInitializer>();
            var order = MapToOrderDto(message);
            await initializer.InitializeProcessingAsync(order, cancellationToken);
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
