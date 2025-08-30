using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderNotificationService.Infrastructure.Messaging.Messages
{
    public record OrderStatusChangedMessage(Guid OrderId, int OrderStatus, string Email);

}
