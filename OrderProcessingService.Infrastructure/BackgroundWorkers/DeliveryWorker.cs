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
        private readonly IEFRepository<ProcessingOrder, Guid> _repository;
        private readonly IOrderServiceApi _orderServiceApi;
        private readonly ILogger<DeliveryWorker> _logger;

        public DeliveryWorker(
            IBackgroundJobClient hangfire,
            IEFRepository<ProcessingOrder, Guid> repository,
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
                //await _repository.AssignUniqueTrackingNumbersAsync(ids, cancellationToken);
                //await _repository.ExecuteUpdateAsync(
                //    calls => calls.SetProperty(po => po.TrackingNumber, Guid.NewGuid().ToString()),
                //    filter: po => ids.Contains(po.Id),
                //    cancellationToken: cancellationToken);

                var processingOrders = await _repository.GetAllAsync(
                    filter: po => ids.Contains(po.Id),
                    asNoTraсking: false,
                    ct: cancellationToken);
       
                _logger.LogInformation("Delivery process for {OrdersCount} orders started.", processingOrders.Count);

                foreach (var po in processingOrders)
                {
                    await Task.Delay(TimeSpan.FromSeconds(30));
                    po.Status = ProcessingStatus.Completed;
                    po.UpdatedAt = DateTime.UtcNow;
                    await _repository.SaveChangesAsync(cancellationToken);
                    await _orderServiceApi.UpdateStatus(po.OrderId, "Delivered", cancellationToken);                   
                    var address = GetRandomAddress();
                    _logger.LogInformation("Delivery with ID: {Id} completed. Order ID: {OrderId}. TrackingNumber: {TrackingNumber}. Address: {Address}", 
                        po.Id, po.OrderId, po.TrackingNumber, address);
                }
                _logger.LogInformation("Delivery successfully completed for all {@OrdersCount} orders.", processingOrders.Count);
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
