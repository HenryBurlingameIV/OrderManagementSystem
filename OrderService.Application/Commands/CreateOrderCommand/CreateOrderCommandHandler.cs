using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using OrderManagementSystem.Shared.Contracts;
using OrderManagementSystem.Shared.Enums;
using OrderService.Application.Contracts;
using OrderService.Application.DTO;
using OrderService.Domain.Entities;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.Commands.CreateOrderCommand
{
    public class CreateOrderCommandHandler(
        IEFRepository<Order, Guid> orderRepository, 
        ICatalogServiceApi catalogServiceClient,
        IValidator<CreateOrderCommand> validator,
        IKafkaProducer<OrderEvent> kafkaOrderProducer,
        IKafkaProducer<OrderStatusEvent> kafkaNotificationProducer,
        ILogger<CreateOrderCommandHandler> logger
        ) : IRequestHandler<CreateOrderCommand, Guid>
    {
        public async Task<Guid> Handle(CreateOrderCommand command, CancellationToken cancellationToken)
        {
            var validationResult = await validator.ValidateAsync(command, cancellationToken);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }
            var orderItemsTasks = command.OrderItems
                .Select(item => CreateAndReserveItemFromRequest(item, cancellationToken))
                .ToList();

            var orderItems = (await Task.WhenAll(orderItemsTasks)).ToList();

            var order = CreateOrder(orderItems, command.Email, DateTime.UtcNow);
            await orderRepository.InsertAsync(order, cancellationToken);
            await orderRepository.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Order with Id {@orderId} was created and saved in database", order.Id);
            await kafkaOrderProducer.ProduceAsync(order.Id.ToString(), CreateOrderEvent(order), cancellationToken);
            logger.LogInformation("Order sent to Kafka. OrderId: {@OrderId}", order.Id);
            await kafkaNotificationProducer.ProduceAsync(
                order.Id.ToString(),
                new OrderStatusEvent(
                    order.Id,
                    (int)order.Status,
                    order.Email
                ),
                cancellationToken);

            logger.LogInformation("OrderStatus sent to Kafka. OrderId: {@OrderId}", order.Id);
            return order.Id;             
        }

        private async Task<OrderItem> CreateAndReserveItemFromRequest(OrderItemRequest request, CancellationToken cancellationToken)
        {
            var product = await catalogServiceClient.ReserveProductAsync(request.Id, request.Quantity, cancellationToken);
            logger.LogInformation("{Quantity} items of product with Id {@productId} was reserved", request.Quantity, request.Id);
            return new OrderItem()
            {
                ProductId = request.Id,
                Quantity = request.Quantity,
                Price = product!.Price,
            };
        }

        private Order CreateOrder(List<OrderItem> orderItems, string email, DateTime createdTime)
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

        private OrderEvent CreateOrderEvent(Order order)
        {
            return new OrderEvent()
            {
                Id = order.Id,
                Status = order.Status.ToString(),
                CreatedAtUtc = order.CreatedAtUtc,
                UpdatedAtUtc = order.UpdatedAtUtc,
                TotalPrice = order.TotalPrice,
                Items = order.Items
                    .Select(p =>
                        new ProductEvent(p.ProductId, p.Price, p.Quantity))
                    .ToList(),
            };
        }

    }
}
