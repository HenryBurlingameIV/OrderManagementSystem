using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.DTO
{
    public record NotificationEvent(Guid OrderId, int OrderStatus, string Email);

}
