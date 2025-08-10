using MediatR;
using Microsoft.AspNetCore.Http.Features;
using OrderManagementSystem.Shared.Contracts;
using OrderManagementSystem.Shared.Exceptions;
using OrderService.Application.DTO;
using OrderService.Domain.Entities;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.Queries.OrderQuery
{
    public class GetOrderByIdQueryHandler(
        IRepository<Order> orderRepository
        ) : IRequestHandler<GetOrderByIdQuery, OrderViewModel>
    {
        public async Task<OrderViewModel> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
        {
            var order = await orderRepository.GetByIdAsync(request.Id, cancellationToken);
            if (order is null)
            {
                throw new NotFoundException($"Order with ID {request.Id} not found.");
            }
            Log.Information("Order with ID {@Id} successfully found", request.Id);
            return CreateOrderViewModel(order);
        }

        public OrderViewModel CreateOrderViewModel(Order order)
        {
            return new OrderViewModel()
            {
                Id = order.Id,
                Status = order.Status.ToString(),
                CreatedAtUtc = order.CreatedAtUtc,
                UpdatedAtUtc = order.UpdatedAtUtc,
                TotalPrice = order.TotalPrice,
                Email = order.Email,
                Items = order.Items
                    .Select(p => 
                        new ProductDto(p.ProductId, p.Price, p.Quantity))
                    .ToList(),
            };
        }

    }
}
