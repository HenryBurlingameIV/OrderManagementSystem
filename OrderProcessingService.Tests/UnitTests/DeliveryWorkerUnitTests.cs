using FluentAssertions;
using Hangfire;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using Moq;
using OrderManagementSystem.Shared.Contracts;
using OrderProcessingService.Application.Contracts;
using OrderProcessingService.Application.DTO;
using OrderProcessingService.Domain.Entities;
using OrderProcessingService.Infrastructure.BackgroundWorkers;
using OrderProcessingService.Tests.ProcessingOrderFixture;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace OrderProcessingService.Tests.UnitTests
{
    public class DeliveryWorkerUnitTests
    {
        private readonly Mock<IEFRepository<ProcessingOrder, Guid>> _mockRepository;
        private readonly Mock<IBackgroundJobClient> _mockBackgroundJobClient;
        private readonly Mock<IOrderServiceApi> _mockOrderServiceApi;
        private readonly Mock<ILogger<DeliveryWorker>> _mockLogger;
        private readonly DeliveryWorker _deliveryWorker;

        public DeliveryWorkerUnitTests()
        {
            _mockRepository = new Mock<IEFRepository<ProcessingOrder, Guid>>();
            _mockBackgroundJobClient = new Mock<IBackgroundJobClient>();
            _mockOrderServiceApi = new Mock<IOrderServiceApi>();
            _mockLogger = new Mock<ILogger<DeliveryWorker>>();
            _deliveryWorker = new DeliveryWorker(
                _mockBackgroundJobClient.Object,
                _mockRepository.Object,
                _mockOrderServiceApi.Object,
                _mockLogger.Object
                );
        }

        [Theory, AutoProcessingOrderData]
        public async Task Should_CompleteDelivery_WhenAllProcessingOrdersExist(List<ProcessingOrder> processingOrders)
        {
            //Arrange
            var stopWatch = new Stopwatch();
            var expectedTime = TimeSpan.FromSeconds(30 * processingOrders.Count);        
            processingOrders.ForEach(po =>
            {
                po.Stage = Stage.Delivery;
                po.Status = ProcessingStatus.Processing;
            });
            var initialUpdatedAt = processingOrders.ToDictionary(po => po.Id, po => po.UpdatedAt);
            var searchIds = processingOrders.Select(po => po.Id).ToList();
            var command = new StartDeliveryCommand(searchIds);
            _mockRepository
                .Setup(repo => repo.GetAllAsync(
                    It.IsAny<Expression<Func<ProcessingOrder, bool>>>(),
                    null,
                    null,
                    false,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(processingOrders);

            //Act
            stopWatch.Start();
            await _deliveryWorker.ProcessAsync(command, CancellationToken.None);
            stopWatch.Stop();

            //Assert
            _mockRepository.VerifyAll();

            _mockRepository
                .Verify(repo => repo
                    .SaveChangesAsync(
                        It.IsAny<CancellationToken>()),
                    Times.Exactly(processingOrders.Count));

            _mockOrderServiceApi
                .Verify(api => api
                    .UpdateStatus(
                        It.Is<Guid>(id => processingOrders.Any(po => po.OrderId == id)),
                        "Delivered",
                        It.IsAny<CancellationToken>()),
                    Times.Exactly(processingOrders.Count));

            _mockLogger
                .Verify(x => x
                    .Log(
                        LogLevel.Information,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, _) => v.ToString().Contains($"Delivery process for {processingOrders.Count} orders started")),
                        null,
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                    Times.Once);

            stopWatch.Elapsed.Should().BeCloseTo(expectedTime, TimeSpan.FromMilliseconds(500));
        }
    }
}
