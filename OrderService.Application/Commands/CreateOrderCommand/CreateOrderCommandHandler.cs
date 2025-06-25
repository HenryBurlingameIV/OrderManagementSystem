using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OrderService.Infrastructure;
using OrderService.Infrastructure.Contracts;
using OrderService.Domain.Entities;
using OrderService.Application.DTO;
using OrderService.Infrastructure.Repositories;

namespace OrderService.Application.Commands.CreateOrderCommand
{
    public class CreateOrderCommandHandler(IRepository<Order> orderRepository, ICatalogServiceClient catalogServiceClient) : IRequestHandler<CreateOrderCommand, Guid>
    {
        public async Task<Guid> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            var orderItemsTasks = request.OrderItems
                .Select(item => CreateAndReserveItemFromRequest(item, cancellationToken))
                .ToList();

            var orderItems = (await Task.WhenAll(orderItemsTasks)).ToList();

            var order = CreateOrder(orderItems, DateTime.UtcNow);
            await orderRepository.CreateAsync(order, cancellationToken);

            return order.Id;             
        }

        private async Task<OrderItem> CreateAndReserveItemFromRequest(OrderItemRequest request, CancellationToken cancellationToken)
        {
            var product = await catalogServiceClient.GetProductByIdAsync(request.Id, cancellationToken);
            await catalogServiceClient.ReserveProductAsync(request.Id, request.Quantity, cancellationToken);
            return new OrderItem()
            {
                ProductId = request.Id,
                Quantity = request.Quantity,
                Price = product.Price,
            };
        }

        private Order CreateOrder(List<OrderItem> orderItems, DateTime createdTime)
        {
            return new Order()
            {
                Id = Guid.NewGuid(),
                Items = orderItems,
                TotalPrice = orderItems.Sum(i => i.Price * i.Quantity),
                Status = OrderStatus.New,
                CreatedAtUtc = createdTime,
                UpdatedAtUtc = createdTime
            };
        }
    }
}
