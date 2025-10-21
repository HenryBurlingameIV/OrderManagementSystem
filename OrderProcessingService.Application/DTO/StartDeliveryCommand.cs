using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderProcessingService.Application.DTO
{
    public record StartDeliveryCommand(IReadOnlyCollection<Guid> ProcessingOrderIds);
}
