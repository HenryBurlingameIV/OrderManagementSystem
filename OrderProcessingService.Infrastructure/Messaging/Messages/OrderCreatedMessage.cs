using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderProcessingService.Infrastructure.Messaging.Messages
{
    public record OrderCreatedMessage(
    Guid Id,
    IReadOnlyList<ProductMessage> Items,
    string Status,
    decimal TotalPrice,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc
    );

}
