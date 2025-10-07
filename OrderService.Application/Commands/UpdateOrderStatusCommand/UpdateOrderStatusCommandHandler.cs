using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using OrderManagementSystem.Shared.Contracts;
using OrderManagementSystem.Shared.Enums;
using OrderManagementSystem.Shared.Exceptions;
using OrderService.Application.Contracts;
using OrderService.Application.DTO;
using OrderService.Application.Services;
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
        IEFRepository<Order, Guid> orderRepository,
        IValidator<OrderStatusValidationModel> validator,
        ICatalogServiceApi catalogServiceClient,
        IKafkaProducer<OrderStatusEvent> kafkaNotificationProducer,
        ILogger<UpdateOrderStatusCommandHandler> logger
        ) : IRequestHandler<UpdateOrderStatusCommand>
    {
        public async Task Handle(UpdateOrderStatusCommand command, CancellationToken cancellationToken)
        {
            var order = await orderRepository.GetByIdAsync(command.Id, cancellationToken);
            if (order is null)
            {
                throw new NotFoundException($"Order with ID {command.Id} not found.");
            }

            logger.LogInformation("Order with ID {@Id} successfully found", command.Id);

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
            await orderRepository.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Status of order with ID {@Id} successfully updated", command.Id);
            await kafkaNotificationProducer.ProduceAsync(
                order.Id.ToString(),
                order.ToOrderStatusEvent(),
                cancellationToken);

            logger.LogInformation("Notification sent to Email: {email}.", order.Email);

        }

        private async Task ReleaseProductsAsync(List<OrderItem> items, CancellationToken cancellationToken)
        {
            foreach (var item in items)
            {
                await catalogServiceClient.ReleaseProductAsync(item.ProductId, item.Quantity, cancellationToken);
                logger.LogInformation("Product {ProductId} released. Quantity: {Quantity}", item.ProductId, item.Quantity);
            };
        }
    }
}
