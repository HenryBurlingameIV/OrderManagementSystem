using OrderManagementSystem.Shared.Enums;
using OrderService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.DTO
{
    public record OrderStatusValidationModel(OrderStatus CurrentStatus, OrderStatus NewStatus);
}
