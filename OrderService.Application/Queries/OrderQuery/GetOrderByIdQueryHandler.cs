using MediatR;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using OrderManagementSystem.Shared.Authorization;
using OrderManagementSystem.Shared.Contracts;
using OrderManagementSystem.Shared.Exceptions;
using OrderService.Application.DTO;
using OrderService.Application.Services;
using OrderService.Domain.Entities;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.Queries.OrderQuery
{
    public class GetOrderByIdQueryHandler(
        IEFRepository<Order, Guid> orderRepository,
        ILogger<GetOrderByIdQueryHandler> logger
        ) : IRequestHandler<GetOrderByIdQuery, OrderViewModel>
    {
        public async Task<OrderViewModel> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
        {
            Expression<Func<Order, bool>> filter = BuildFilter(request.Id, request.CustomerId);

            var order = await orderRepository.GetFirstOrDefaultAsync(
                selector: o => o.ToViewModel(),
                filter: filter,
                ct: cancellationToken);

            if (order is null)
            {
                throw new NotFoundException($"Order with ID {request.Id} not found.");
            }

            logger.LogInformation("Order with ID {@Id} successfully found", request.Id);
            return order;
        }

        private Expression<Func<Order, bool>> BuildFilter(Guid orderId, Guid? customerId) =>
            customerId.HasValue
                ? o => o.Id == orderId && o.CustomerId == customerId
                : o => o.Id == orderId;
    }
}
