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
    public class MessageHandler(IOrderProcessingInitializer initializer) : IMessageHandler<OrderCreatedMessage>
    {
        public async Task HandleAsync(OrderCreatedMessage message, CancellationToken cancellationToken)
        {
            var order = GetOrderFromMessage(message);
            await initializer.InitializeProcessingAsync(order, cancellationToken);
        }

        private OrderDto GetOrderFromMessage(OrderCreatedMessage message)
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
