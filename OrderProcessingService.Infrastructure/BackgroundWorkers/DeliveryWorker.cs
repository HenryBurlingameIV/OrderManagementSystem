using Bogus;
using Hangfire;
using Microsoft.Extensions.Logging;
using OrderManagementSystem.Shared.Contracts;
using OrderProcessingService.Application.Contracts;
using OrderProcessingService.Application.DTO;
using OrderProcessingService.Domain.Entities;
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

        public async Task ProcessAsync(List<Guid> ids, CancellationToken cancellationToken)
        {
            try
            {
                await _repository.BulkUpdateProcessingOrdersTrackingAsync(ids, Guid.NewGuid().ToString(), cancellationToken);
                var processingOrders = await _repository.GetByIdsAsync(ids, cancellationToken);
       
                _logger.LogInformation("Delivery for {OrdersCount} orders started.", processingOrders.Count);

                foreach (var po in processingOrders)
                {
                    await Task.Delay(TimeSpan.FromSeconds(30));
                    var address = GetRandomAddress();
                    _logger.LogInformation("Delivery with ID: {Id} completed. Order ID: {OrderId}. TrackingNumber: {TrackingNumber}. Address: {Address}", 
                        po.Id, po.OrderId, po.TrackingNumber, address);
                    po.Status = ProcessingStatus.Completed;
                    po.UpdatedAt = DateTime.UtcNow;
                    await _repository.UpdateAsync(po, cancellationToken);
                    await _orderServiceApi.UpdateStatus(po.OrderId, "Delivered", cancellationToken);
                    _logger.LogInformation("Delivery for {OrdersCount} orders completed.", processingOrders.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Delivery failed.");
            }
        }

        public string GetRandomAddress()
        {
            var faker = new Faker("ru");
            return $"г. {faker.Address.City()}, ул. {faker.Address.StreetName()}, д. {faker.Random.Number(100)}, кв. {faker.Random.Number(500)}";
        }
    }
}
