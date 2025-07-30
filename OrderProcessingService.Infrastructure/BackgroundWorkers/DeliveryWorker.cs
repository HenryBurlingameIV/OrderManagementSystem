using Hangfire;
using Microsoft.Extensions.Logging;
using OrderManagementSystem.Shared.Contracts;
using OrderProcessingService.Application.Contracts;
using OrderProcessingService.Application.DTO;
using OrderProcessingService.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace OrderProcessingService.Infrastructure.BackgroundWorkers
{
    public class DeliveryWorker : IOrderBackgroundWorker<StartDeliveryCommand>
    {
        private readonly IBackgroundJobClient _hangfire;
        private readonly IProcessingOrderRepository _repository;
        private readonly IOrderServiceApi _orderServiceApi;
        private readonly ILogger<DeliveryWorker> _logger;

        public DeliveryWorker(
            IBackgroundJobClient hangfire,
            IProcessingOrderRepository repository,
            IOrderServiceApi orderServiceApi,
            ILogger<DeliveryWorker> logger
            ) 
        {
            _hangfire = hangfire;
            _repository = repository;
            _orderServiceApi = orderServiceApi;
            _logger = logger;
        }
        public Task ScheduleAsync(StartDeliveryCommand command, CancellationToken cancellationToken)
        {
            _hangfire.Schedule(() => ProcessAsync(command.ProcessingOrderIds, cancellationToken), TimeSpan.FromSeconds(60));
            return Task.CompletedTask;
        }

        private async Task ProcessAsync(List<Guid> ids, CancellationToken cancellationToken)
        {

        }
    }
}
