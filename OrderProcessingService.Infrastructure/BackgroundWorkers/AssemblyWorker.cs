using Hangfire;
using Hangfire.Client;
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
        private readonly IRepository<ProcessingOrder> _repository;
        private readonly IOrderServiceApi _orderServiceApi;
        private readonly ILogger<AssemblyWorker> _logger;

        public AssemblyWorker(
            IBackgroundJobClient hangfire,
            IRepository<ProcessingOrder> repository,
            IOrderServiceApi orderServiceApi,
            ILogger<AssemblyWorker> logger
            )
        {
            _hangfire = hangfire;
            _repository = repository;
            _orderServiceApi = orderServiceApi;
            _logger = logger;
        }
        public Task ScheduleAsync(StartAssemblyCommand command, CancellationToken cancellationToken)
        {
            _hangfire.Schedule(() => ProcessAsync(command.ProcessingOrderId, cancellationToken), TimeSpan.FromSeconds(60));
            return Task.CompletedTask;
        }

        public async Task ProcessAsync(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var processingOrder = await _repository.GetByIdAsync(id, cancellationToken);
                _logger.LogInformation("Starting assembly with ID {id}. Order ID is {OrderId}", processingOrder!.Id, processingOrder.OrderId);
                foreach (var item in processingOrder.Items)
                {
                    await Task.Delay(TimeSpan.FromSeconds(30));
                    item.Status = ItemAssemblyStatus.Ready;
                    await _repository.UpdateAsync(processingOrder, cancellationToken); //не уверен стоит ли обновлять репозиторий на каждой итерации
                    _logger.LogInformation("Order item with ID {id} is ready", item.ProductId);
                }

                processingOrder.Status = ProcessingStatus.Completed;
                await _repository.UpdateAsync(processingOrder, cancellationToken);
                await _orderServiceApi.UpdateStatus(processingOrder.OrderId, "Ready", cancellationToken);


                _logger.LogInformation("Assembly processing with ID {id} completed. Order ID is {OrderId}", processingOrder.Id, processingOrder.OrderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Assembly processing with ID {id} failed.", id);
            }

        }
    }
}
