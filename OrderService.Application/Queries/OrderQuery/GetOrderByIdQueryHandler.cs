using MediatR;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using OrderManagementSystem.Shared.Contracts;
using OrderManagementSystem.Shared.Exceptions;
using OrderService.Application.DTO;
using OrderService.Application.Services;
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
        IRepositoryBase<Order, Guid> orderRepository,
        ILogger<GetOrderByIdQueryHandler> logger
        ) : IRequestHandler<GetOrderByIdQuery, OrderViewModel>
    {
        public async Task<OrderViewModel> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
        {
            var order = await orderRepository.GetByIdAsync(request.Id, cancellationToken);
            if (order is null)
            {
                throw new NotFoundException($"Order with ID {request.Id} not found.");
            }

            logger.LogInformation("Order with ID {@Id} successfully found", request.Id);
            return order.ToViewModel();
        }
    }
}
