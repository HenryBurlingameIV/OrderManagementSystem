using OrderProcessingService.Application.DTO;
using OrderProcessingService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace OrderProcessingService.Application.Services
{
    public static class ProcessingOrderMapper
    {
        public static ProcessingOrderViewModel ToViewModel(this ProcessingOrder processingOrder)
        {
            processingOrder.TrackingNumber ??= "unassigned";
            return new ProcessingOrderViewModel(
                processingOrder.Id,
                processingOrder.OrderId,
                processingOrder.Stage.ToString(),
                processingOrder.Status.ToString(),
                processingOrder.TrackingNumber,
                processingOrder.CreatedAt,
                processingOrder.UpdatedAt
                );
        }

        public static ProcessingOrder ToEntity(this OrderDto dto)
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
