using MediatR;
using OrderManagementSystem.Shared.Contracts;
using OrderManagementSystem.Shared.Exceptions;
using OrderService.Domain.Entities;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.Commands.UpdateOrderStatusCommand
{
    public class UpdateOrderStatusCommandHandler(
        IRepository<Order> orderRepository
        ) : IRequestHandler<UpdateOrderStatusCommand>
    {
        public async Task Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
        {
            var order = await orderRepository.GetByIdAsync(request.Id, cancellationToken);
            if (order is null)
            {
                throw new NotFoundException($"Order with ID {request.Id} not found.");
            }

            Log.Information("Order with ID {@Id} successfully found", request.Id);

            var newStatus = Enum.Parse<OrderStatus>(request.NewOrderStatus);

            order.Status = newStatus;
            await orderRepository.UpdateAsync(order, cancellationToken);
            Log.Information("Status of order with ID {@Id} successfully updated", request.Id);

        }
    }
}
