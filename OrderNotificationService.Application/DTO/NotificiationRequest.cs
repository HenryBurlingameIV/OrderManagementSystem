using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderNotificationService.Application.DTO
{
    public record NotificationRequest(Guid OrderId, int OrderStatus, string Email);

}
