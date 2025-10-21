using OrderProcessingService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderProcessingService.Application.DTO
{
    public record ProcessingOrderViewModel(
        Guid Id, 
        Guid OrderId, 
        string Stage, 
        string Status, 
        string TrackingNumber, 
        DateTime CreatedAt, 
        DateTime UpdatedAt);

}
