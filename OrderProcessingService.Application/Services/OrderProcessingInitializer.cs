using Microsoft.AspNetCore.DataProtection.Repositories;
using OrderManagementSystem.Shared.Contracts;
using OrderProcessingService.Application.Contracts;
using OrderProcessingService.Application.DTO;
using OrderProcessingService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderProcessingService.Application.Services
{
    public class OrderProcessingInitializer(IRepository<ProcessingOrder> repository) : IOrderProcessingInitializer
    {
        public async Task InitializeProcessingAsync(OrderDto dto, CancellationToken cancellationToken)
        {
            var processingOrder = GetProcessingOrder(dto);
            await repository.CreateAsync(processingOrder, cancellationToken);            
        }

        public ProcessingOrder GetProcessingOrder(OrderDto dto)
        {
            return new ProcessingOrder()
            {
                Id = Guid.NewGuid(),
                OrderId = dto.Id,
                CreatedAt = dto.CreatedAt,
                UpdatedAt = dto.UpdatedAt,
                Stage = Stage.Assembly,
                Status = ProcessingStatus.New,
                TrackingNumber = null,
                Items = dto.Items
                    .Select(i => new OrderItem()
                    {
                        ProductId = i.ProductId,
                        Quantity = i.Quantity,
                        Status = ItemAssemblyStatus.Pending,
                    })
                    .ToList(),

            };
        }
    }
}
