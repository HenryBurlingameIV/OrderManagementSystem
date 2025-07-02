using FluentValidation;
using MediatR;
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

namespace OrderService.Application.Commands.UpdateOrderStatusCommand
{
    public class UpdateOrderStatusCommandHandler(
        IRepository<Order> orderRepository,
        IValidator<OrderStatusValidationModel> validator
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

            var validationResult = await validator.ValidateAsync(
                new OrderStatusValidationModel(order.Status, request.NewOrderStatus), cancellationToken);
            if(!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var newStatus = Enum.Parse<OrderStatus>(request.NewOrderStatus, true);

            order.Status = newStatus;
            await orderRepository.UpdateAsync(order, cancellationToken);
            Log.Information("Status of order with ID {@Id} successfully updated", request.Id);

        }
    }
}
