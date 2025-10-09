using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using OrderManagementSystem.Shared.Contracts;
using OrderManagementSystem.Shared.Enums;
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

namespace OrderService.Application.Commands.CreateOrderCommand
{
    public class CreateOrderCommandHandler(
        IEFRepository<Order, Guid> orderRepository, 
        OrderFactory orderFactory,
        IValidator<CreateOrderRequest> validator,
        IKafkaProducer<OrderEvent> kafkaOrderProducer,
        IKafkaProducer<OrderStatusEvent> kafkaOrderStatusProducer,
        ILogger<CreateOrderCommandHandler> logger
        ) : IRequestHandler<CreateOrderCommand, Guid>
    {
        public async Task<Guid> Handle(CreateOrderCommand command, CancellationToken cancellationToken)
        {
            var validationResult = await validator.ValidateAsync(command.Request, cancellationToken);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var order = await orderFactory.CreateOrderAsync(command.Request, cancellationToken);
            await orderRepository.InsertAsync(order, cancellationToken);
            await orderRepository.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Order with Id {@orderId} was created and saved in database", order.Id);
            await kafkaOrderProducer.ProduceAsync(order.Id.ToString(), order.ToOrderEvent(), cancellationToken);
            logger.LogInformation("Order sent to Kafka. OrderId: {@OrderId}", order.Id);
            await kafkaOrderStatusProducer.ProduceAsync(
                order.Id.ToString(),
                order.ToOrderStatusEvent(),
                cancellationToken);

            logger.LogInformation("OrderStatus sent to Kafka. OrderId: {@OrderId}", order.Id);
            return order.Id;             
        }
    }
}
