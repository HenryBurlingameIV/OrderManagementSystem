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
        private readonly IRepository<ProcessingOrder> _repository;
        private readonly IOrderBackgroundWorker<StartAssemblyCommand> _assemblyWorker;
        private readonly IOrderServiceApi _orderServiceApi;
        private readonly ILogger<OrderProcessor> _logger;
        private readonly IValidator<StartAssemblyStatus> _validator;

        public OrderProcessor(
            IRepository<ProcessingOrder> repository, 
            IOrderBackgroundWorker<StartAssemblyCommand> assemblyWorker,
            IOrderServiceApi orderServiceApi,
            ILogger<OrderProcessor> logger,
            IValidator<StartAssemblyStatus> validator
            )
        {
            _repository = repository;
            _assemblyWorker = assemblyWorker;
            _orderServiceApi = orderServiceApi;
            _logger = logger;
            _validator = validator;
        }
        public async Task BeginAssembly(Guid id, CancellationToken cancellationToken)
        {
            var po = await _repository.GetByIdAsync(id, cancellationToken);
            if(po is null)
            {
                throw new NotFoundException($"Processing order with ID {id} not found.");
            }
            _logger.LogInformation("Processing order with ID {@Id} successfully found", id);
            await _validator.ValidateAndThrowAsync(new StartAssemblyStatus(po.Stage, po.Status), cancellationToken);

            po.Status = ProcessingStatus.Processing;
            await _repository.UpdateAsync(po, cancellationToken);
            await _orderServiceApi.UpdateStatus(po.OrderId, "Processing", cancellationToken);

            await _assemblyWorker.ScheduleAsync(new StartAssemblyCommand(po.Id), cancellationToken);
            _logger.LogInformation("Processing order with ID {@Id} scheduled", id);
        }

        public Task BeginDelivery(List<Guid> ids, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
