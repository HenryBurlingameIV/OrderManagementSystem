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
        public async Task Handle(UpdateOrderStatusCommand command, CancellationToken cancellationToken)
        {
            var order = await orderRepository.GetByIdAsync(command.Id, cancellationToken);
            if (order is null)
            {
                throw new NotFoundException($"Order with ID {command.Id} not found.");
            }

            Log.Information("Order with ID {@Id} successfully found", command.Id);

            var validationResult = await validator.ValidateAsync(
                new OrderStatusValidationModel(order.Status, command.NewOrderStatus), cancellationToken);
            if(!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            order.Status = command.NewOrderStatus;
            await orderRepository.UpdateAsync(order, cancellationToken);
            Log.Information("Status of order with ID {@Id} successfully updated", command.Id);

        }
    }
}
