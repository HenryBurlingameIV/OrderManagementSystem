using OrderService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.DTO
{
    public record OrderViewModel(
        Guid Id,
        List<ProductDto> Items,
        string Status,
        decimal TotalPrice,
        DateTime CreatedAtUtc,
        DateTime UpdatedAtUtc,
        string Email
        );

}
