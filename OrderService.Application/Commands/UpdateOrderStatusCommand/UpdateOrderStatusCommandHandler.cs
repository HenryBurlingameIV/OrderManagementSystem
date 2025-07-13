using FluentValidation;
using MediatR;
using OrderManagementSystem.Shared.Contracts;
using OrderManagementSystem.Shared.Exceptions;
using OrderService.Application.Contracts;
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
        IValidator<OrderStatusValidationModel> validator,
        ICatalogServiceApi catalogServiceClient
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

            if(command.NewOrderStatus == OrderStatus.Cancelled)
            {
                await ReleaseProductsAsync(order.Items, cancellationToken);
            }

            order.Status = command.NewOrderStatus;
            order.UpdatedAtUtc = DateTime.UtcNow;
            await orderRepository.UpdateAsync(order, cancellationToken);
            Log.Information("Status of order with ID {@Id} successfully updated", command.Id);

        }

        private async Task ReleaseProductsAsync(List<OrderItem> items, CancellationToken cancellationToken)
        {
            foreach (var item in items)
            {
                await catalogServiceClient.ReleaseProductAsync(item.ProductId, item.Quantity, cancellationToken);
                Log.Information("Product {ProductId} released. Quantity: {Quantity}", item.ProductId, item.Quantity);
            };
        }
    }
}
