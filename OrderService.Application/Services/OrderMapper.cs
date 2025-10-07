using OrderManagementSystem.Shared.Enums;
using OrderService.Application.DTO;
using OrderService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.Services
{
    public static class OrderMapper
    {
        public static Order ToEntity(List<OrderItem> orderItems, string email, DateTime createdTime)
        {
            return new Order()
            {
                Id = Guid.NewGuid(),
                Items = orderItems,
                TotalPrice = orderItems.Sum(i => i.Price * i.Quantity),
                Status = OrderStatus.New,
                CreatedAtUtc = createdTime,
                UpdatedAtUtc = createdTime,
                Email = email,
            };
        }

        public static OrderEvent ToOrderEvent(this Order order)
        {
            return new OrderEvent()
            {
                Id = order.Id,
                Status = order.Status.ToString(),
                CreatedAtUtc = order.CreatedAtUtc,
                UpdatedAtUtc = order.UpdatedAtUtc,
                TotalPrice = order.TotalPrice,
                Items = order.Items
                    .Select(item => new ProductEvent(item.ProductId, item.Price, item.Quantity))
                    .ToList(),
            };
        }

        public static OrderStatusEvent ToOrderStatusEvent(this Order order)
        {
            return new OrderStatusEvent(
                order.Id,
                (int)order.Status,
                order.Email
            );
        }
    }
}
