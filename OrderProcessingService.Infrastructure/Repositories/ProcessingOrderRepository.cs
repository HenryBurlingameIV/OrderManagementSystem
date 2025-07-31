using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using OrderManagementSystem.Shared.Contracts;
using OrderProcessingService.Application.Contracts;
using OrderProcessingService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderProcessingService.Infrastructure.Repositories
{
    public class ProcessingOrderRepository(OrderProcessingDbContext dbContext) : IProcessingOrderRepository
    {
        public async Task<Guid> CreateAsync(ProcessingOrder item, CancellationToken cancellationToken)
        {
            await dbContext.AddAsync(item, cancellationToken);
            await dbContext.SaveChangesAsync();
            return item.Id;
        }


        public async Task<ProcessingOrder?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var processingOrder = await dbContext.ProcessingOrders
                .Include(po => po.Items)
                .FirstOrDefaultAsync(po => po.Id == id, cancellationToken);
            return processingOrder;
        }

        public async Task<Guid> UpdateAsync(ProcessingOrder item, CancellationToken cancellationToken)
        {
            dbContext.Entry(item).State = EntityState.Modified;
            await dbContext.SaveChangesAsync();
            return item.Id;
        }
        public Task DeleteAsync(ProcessingOrder item, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task UpdateItemsAssemblyStatusAsync(Guid proccessingOrderId, ItemAssemblyStatus newStatus, CancellationToken cancellationToken)
        {
            await dbContext.ProcessingOrders
                .Where(po => po.Id == proccessingOrderId)
                .SelectMany(po => po.Items)
                .ExecuteUpdateAsync(i => i.SetProperty(p => p.Status, newStatus));               
        }

        public async Task BulkUpdateProcessingOrdersStatusAsync(IEnumerable<Guid> ids, ProcessingStatus newStatus, Stage newStage, CancellationToken cancellationToken)
        {
            var updatedAt = DateTime.UtcNow;
            await dbContext.ProcessingOrders
                .Where(po => ids.Contains(po.Id))
                .ExecuteUpdateAsync(po => po
                    .SetProperty(po => po.Status, newStatus)
                    .SetProperty(po => po.Stage, newStage)
                    .SetProperty(po => po.UpdatedAt, updatedAt)
                    );
        }

        public async Task<List<ProcessingOrder>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken)
        {
            return await dbContext.ProcessingOrders
                .Where(po => ids.Contains(po.Id))
                .ToListAsync(cancellationToken);
        }

        public async Task BulkUpdateProcessingOrdersTrackingAsync(IEnumerable<Guid> ids, string trackingNumber, CancellationToken cancellationToken)
        {
            var updatedAt = DateTime.UtcNow;
            await dbContext.ProcessingOrders
                .Where(po => ids.Contains(po.Id))
                .ExecuteUpdateAsync(po => po
                    .SetProperty(po => po.TrackingNumber, trackingNumber)
                    .SetProperty(po => po.UpdatedAt, updatedAt));
        }
    }
}
