using OrderManagementSystem.Shared.Contracts;
using OrderProcessingService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderProcessingService.Application.Contracts
{
    public interface IProcessingOrderRepository : IRepository<ProcessingOrder>
    {
        Task UpdateItemsAssemblyStatusAsync(Guid proccessingOrderId, ItemAssemblyStatus newStatus, CancellationToken cancellationToken);
    }
}
