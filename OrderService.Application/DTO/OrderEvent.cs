using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.DTO
{
    public record OrderEvent(
        Guid Id, 
        List<ProductEvent> Items, 
        string Status, 
        decimal TotalPrice, 
        DateTime CreatedAtUtc, 
        DateTime UpdatedAtUtc);
}
