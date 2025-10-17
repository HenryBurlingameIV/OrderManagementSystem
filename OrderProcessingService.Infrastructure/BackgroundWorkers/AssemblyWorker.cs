using Hangfire;
using Hangfire.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderManagementSystem.Shared.Contracts;
using OrderProcessingService.Application.Contracts;
using OrderProcessingService.Application.DTO;
using OrderProcessingService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderProcessingService.Infrastructure.BackgroundWorkers
{
    public class AssemblyWorker : IOrderBackgroundWorker<StartAssemblyCommand>
    {
        private readonly IBackgroundJobClient _hangfire;
        private readonly IEFRepository<ProcessingOrder,Guid> _processingOrdersRepository;
        private readonly IEFRepository<OrderItem, Guid> _orderItemsRepository;
        private readonly IOrderServiceApi _orderServiceApi;
        private readonly ILogger<AssemblyWorker> _logger;

        public AssemblyWorker(
            IBackgroundJobClient hangfire,
            IEFRepository<ProcessingOrder, Guid> processingOrdersRepository,
            IEFRepository<OrderItem, Guid> orderItemsRepository,
            IOrderServiceApi orderServiceApi,
            ILogger<AssemblyWorker> logger
            )
        {
            _hangfire = hangfire;
            _processingOrdersRepository = processingOrdersRepository;
            _orderItemsRepository = orderItemsRepository;
            _orderServiceApi = orderServiceApi;
            _logger = logger;
        }
        public Task ScheduleAsync(StartAssemblyCommand command, CancellationToken cancellationToken)
        {
            _hangfire.Schedule(() => ProcessAsync(command, cancellationToken), TimeSpan.FromSeconds(60));
            return Task.CompletedTask;
        }

        public async Task ProcessAsync(StartAssemblyCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var processingOrder = await _processingOrdersRepository.GetFirstOrDefaultAsync(
                    filter: po => po.Id == command.ProcessingOrderId,
                    include: (po) => po.Include(po => po.Items),
                    asNoTraсking: false,
                    ct: cancellationToken);

                if (processingOrder == null)
                {
                    _logger.LogError("Processing order with ID {@ProcessingOrderId} not found", command.ProcessingOrderId);
                    return;
                }

                _logger.LogInformation("Starting assembly with ID {@ProcessingOrderId}. Order ID is {OrderId}", processingOrder!.Id, processingOrder.OrderId);
                foreach (var item in processingOrder.Items)
                {
                    await Task.Delay(TimeSpan.FromSeconds(30));
                    _logger.LogInformation("Order item with ID {id} is ready", item.ProductId);
                }
               
                await _orderItemsRepository.ExecuteUpdateAsync(
                    setPropertyCalls: calls => calls.SetProperty(item => item.Status, ItemAssemblyStatus.Ready),
                    filter: item => item.ProcessingOrderId == command.ProcessingOrderId,
                    cancellationToken: cancellationToken
                    );

                _logger.LogInformation("Updated {Count} items to Ready status", processingOrder.Items.Count);

                await _orderServiceApi.UpdateStatus(processingOrder.OrderId, "Ready", cancellationToken);
                processingOrder.Status = ProcessingStatus.Completed;
                processingOrder.UpdatedAt = DateTime.UtcNow;
                await _processingOrdersRepository.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Assembly processing with ID {@ProcessingOrderId} completed. Order ID is {@OrderId}", processingOrder.Id, processingOrder.OrderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Assembly processing with ID {@ProcessingOrderId} failed.", command.ProcessingOrderId);
            }

        }
    }
}
