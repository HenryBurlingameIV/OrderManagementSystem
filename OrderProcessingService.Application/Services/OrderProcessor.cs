using FluentValidation;
using Microsoft.Extensions.Logging;
using OrderManagementSystem.Shared.Contracts;
using OrderManagementSystem.Shared.Exceptions;
using OrderProcessingService.Application.Contracts;
using OrderProcessingService.Application.DTO;
using OrderProcessingService.Domain.Entities;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ValidationException = FluentValidation.ValidationException;

namespace OrderProcessingService.Application.Services
{
    public class OrderProcessor : IOrderProcessor
    {
        private readonly IProcessingOrderRepository _repository;
        private readonly IOrderBackgroundWorker<StartAssemblyCommand> _assemblyWorker;
        private readonly IOrderServiceApi _orderServiceApi;
        private readonly ILogger<OrderProcessor> _logger;
        private readonly IValidator<StartAssemblyStatus> _assemblyStatusValidator;
        private readonly IValidator<StartDeliveryStatus> _deliveryStatusValidator;
        private readonly IOrderBackgroundWorker<StartDeliveryCommand> _deliveryWorker;

        public OrderProcessor(
            IProcessingOrderRepository repository, 
            IOrderBackgroundWorker<StartAssemblyCommand> assemblyWorker,
            IOrderBackgroundWorker<StartDeliveryCommand> deliveryWorker,
            IOrderServiceApi orderServiceApi,
            ILogger<OrderProcessor> logger,
            IValidator<StartAssemblyStatus> assemblyStatusValidator,
            IValidator<StartDeliveryStatus> deliveryStatusValidator
            )
        {
            _repository = repository;
            _assemblyWorker = assemblyWorker;
            _orderServiceApi = orderServiceApi;
            _logger = logger;
            _assemblyStatusValidator = assemblyStatusValidator;
            _deliveryStatusValidator = deliveryStatusValidator;
            _deliveryWorker = deliveryWorker;
        }
        public async Task BeginAssembly(Guid id, CancellationToken cancellationToken)
        {
            var po = await _repository.GetByIdAsync(id, cancellationToken);
            if(po is null)
            {
                throw new NotFoundException($"Processing order with ID {id} not found.");
            }
            _logger.LogInformation("Processing order with ID {@Id} successfully found", id);
            await _assemblyStatusValidator.ValidateAndThrowAsync(new StartAssemblyStatus(po.Stage, po.Status), cancellationToken);

            await _orderServiceApi.UpdateStatus(po.OrderId, "Processing", cancellationToken);
            po.Status = ProcessingStatus.Processing;
            po.UpdatedAt = DateTime.UtcNow;
            await _repository.UpdateAsync(po, cancellationToken);

            await _assemblyWorker.ScheduleAsync(new StartAssemblyCommand(po.Id), cancellationToken);
            _logger.LogInformation("Processing order with ID {@Id} scheduled", id);
        }

        public async Task BeginDelivery(List<Guid> ids, CancellationToken cancellationToken)
        {
            var processingOrders = await _repository.GetByIdsAsync(ids, cancellationToken);
            if (processingOrders.Count != ids.Count)
            {
                var missingIds = ids.Except(processingOrders.Select(x => x.Id));
                throw new NotFoundException($"Processing orders not found. Missing IDs:\n{string.Join("\n", missingIds)}");
            }
            
            foreach (var po in processingOrders)
                await _deliveryStatusValidator.ValidateAndThrowAsync(new StartDeliveryStatus(po.Stage, po.Status), cancellationToken);

            await _repository.BulkUpdateProcessingOrdersStatusAsync(ids, 
                ProcessingStatus.Processing, 
                Stage.Delivery, 
                cancellationToken);

            foreach(var po in processingOrders)
            {
                await _orderServiceApi.UpdateStatus(po.OrderId, "Delivering", cancellationToken);
            }

            await _deliveryWorker.ScheduleAsync(new StartDeliveryCommand(ids), cancellationToken);
            _logger.LogInformation("Delivery for {OrdersCount} orders scheduled.", ids.Count);
        }
    }
}
