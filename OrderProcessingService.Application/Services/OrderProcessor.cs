using OrderManagementSystem.Shared.Contracts;
using OrderManagementSystem.Shared.Exceptions;
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
    public class OrderProcessor : IOrderProcessor
    {
        private readonly IRepository<ProcessingOrder> _repository;
        private readonly IOrderBackgroundWorker<StartAssemblyCommand> _assemblyWorker;

        public OrderProcessor(IRepository<ProcessingOrder> repository, IOrderBackgroundWorker<StartAssemblyCommand> assemblyWorker)
        {
            _repository = repository;
            _assemblyWorker = assemblyWorker;
            
        }
        public async Task BeginAssembly(Guid id, CancellationToken cancellationToken)
        {
            var op = await _repository.GetByIdAsync(id, cancellationToken);
            if(op is null)
            {
                throw new NotFoundException($"Processing order with ID {id} not found.");
            }

            Log.Information("Processing order with ID {@Id} successfully found", id);
            await _assemblyWorker.ScheduleAsync(new StartAssemblyCommand(op), cancellationToken);
        }

        public Task BeginDelivery(List<Guid> ids, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
