using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.Extensions.Logging;
using OrderManagementSystem.Shared.Contracts;
using OrderProcessingService.Application.Contracts;
using OrderProcessingService.Application.DTO;
using OrderProcessingService.Domain.Entities;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderProcessingService.Application.Services
{
    public class OrderProcessingInitializer(
        IEFRepository<ProcessingOrder, Guid> repository,
        ILogger<OrderProcessingInitializer> logger
        ) : IOrderProcessingInitializer
    {
        public async Task InitializeProcessingAsync(OrderDto dto, CancellationToken cancellationToken)
        {
            var processingOrder = CreateFromOrderDto(dto);
            await repository.InsertAsync(processingOrder, cancellationToken);
            await repository.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Processing with ID {@ProcessingOrderId} initialized. Order ID: {@OrderId}", processingOrder.Id, processingOrder.OrderId);
        }

        public ProcessingOrder CreateFromOrderDto(OrderDto dto)
        {
            var ProcessinOrderId = Guid.NewGuid();

            return new ProcessingOrder()
            {
                Id = ProcessinOrderId,
                OrderId = dto.Id,
                CreatedAt = dto.CreatedAt,
                UpdatedAt = dto.UpdatedAt,
                Stage = Stage.Assembly,
                Status = ProcessingStatus.New,
                TrackingNumber = null,
                Items = dto.Items
                    .Select(i => new OrderItem()
                    {
                        Id = Guid.NewGuid(),
                        ProcessingOrderId = ProcessinOrderId,
                        ProductId = i.ProductId,
                        Quantity = i.Quantity,
                        Status = ItemAssemblyStatus.Pending,
                    })
                    .ToList(),
            };
        }
    }
}
