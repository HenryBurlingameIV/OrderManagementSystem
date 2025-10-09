using MediatR;
using Microsoft.Extensions.Logging;
using OrderManagementSystem.Shared.Enums;
using OrderService.Application.Contracts;
using OrderService.Application.DTO;
using OrderService.Domain.Entities;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace OrderService.Application.Services
{
    public class OrderFactory
    {
        private readonly ICatalogServiceApi _catalogServiceApi;
        private readonly ILogger<OrderFactory> _logger;

        public OrderFactory(ICatalogServiceApi catalogServiceApi, ILogger<OrderFactory> logger)
        {
            _catalogServiceApi = catalogServiceApi;
            _logger = logger;
        }

        public async Task<Order> CreateOrderAsync(CreateOrderRequest request, CancellationToken ct)
        {
            var orderItems = await CreateOrderItemsAsync(request.Items, ct);
            var createdAt = DateTime.UtcNow;
            return new Order()
            {
                Id = Guid.NewGuid(),
                Items = orderItems,
                TotalPrice = orderItems.Sum(i => i.Price * i.Quantity),
                Status = OrderStatus.New,
                CreatedAtUtc = createdAt,
                UpdatedAtUtc = createdAt,
                Email = request.Email
            };
        }

        private async Task<OrderItem> CreateOrderItemAsync(OrderItemRequest request, CancellationToken ct)
        {
            var product = await _catalogServiceApi.ReserveProductAsync(request.Id, request.Quantity, ct);
            _logger.LogInformation("{Quantity} items of product with Id {@productId} was reserved", request.Quantity, request.Id);
            return new OrderItem()
            {
                ProductId = request.Id,
                Quantity = request.Quantity,
                Price = product!.Price,
            };
        }

        private async Task<List<OrderItem>> CreateOrderItemsAsync(IEnumerable<OrderItemRequest> requests, CancellationToken ct)
        {
            var orderItemsTasks = requests
                .Select(item => CreateOrderItemAsync(item, ct))
                .ToList();

            var orderItems = (await Task.WhenAll(orderItemsTasks)).ToList();
            return orderItems;
        }
    }
}
