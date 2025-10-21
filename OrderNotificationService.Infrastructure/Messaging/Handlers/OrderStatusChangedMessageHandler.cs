using OrderManagementSystem.Shared.Contracts;
using OrderNotificationService.Application.Contracts;
using OrderNotificationService.Application.DTO;
using OrderNotificationService.Infrastructure.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderNotificationService.Infrastructure.Messaging.Handlers
{
    public class OrderStatusChangedMessageHandler : IMessageHandler<OrderStatusChangedMessage>
    {
        private readonly INotificationService<NotificationRequest> _orderNotificationService;

        public OrderStatusChangedMessageHandler(INotificationService<NotificationRequest> orderNotificationService)
        {
            _orderNotificationService = orderNotificationService;
        }
        public async Task HandleAsync(OrderStatusChangedMessage message, CancellationToken cancellationToken)
        {
            var request = new NotificationRequest(message.OrderId, message.OrderStatus, message.Email);
            await _orderNotificationService.NotifyAsync(request, cancellationToken);
        }
    }
}
