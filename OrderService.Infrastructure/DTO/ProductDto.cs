using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Infrastructure.DTO
{
    public record ProductDto(Guid Id, decimal Price, int AvailableQuantity);
}
