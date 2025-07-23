using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderProcessingService.Infrastructure.Messaging.Messages
{
    public record ProductMessage(Guid Id, decimal Price, int Quantity);
}
