using Castle.Core.Logging;
using FluentAssertions;
using Hangfire;
using Microsoft.Extensions.Logging;
using Moq;
using OrderProcessingService.Application.Contracts;
using OrderProcessingService.Application.DTO;
using OrderProcessingService.Domain.Entities;
using OrderProcessingService.Infrastructure.BackgroundWorkers;
using OrderProcessingService.Tests.ProcessingOrderFixture;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderProcessingService.Tests.UnitTests
{
    public class AssemblyWorkerUnitTests
    {
        private readonly Mock<IProcessingOrderRepository> _mockRepository;
        private readonly Mock<IBackgroundJobClient> _mockBackgroundJobClient;
        private readonly Mock<IOrderServiceApi> _mockOrderServiceApi;
        private readonly Mock<ILogger<AssemblyWorker>> _mockLogger;
        private readonly AssemblyWorker _asseblyWorker;

        public AssemblyWorkerUnitTests() 
        {
            _mockRepository = new Mock<IProcessingOrderRepository>();
            _mockBackgroundJobClient = new Mock<IBackgroundJobClient>();
            _mockOrderServiceApi = new Mock<IOrderServiceApi>();
            _mockLogger = new Mock<ILogger<AssemblyWorker>>();
            _asseblyWorker = new AssemblyWorker(
                _mockBackgroundJobClient.Object,
                _mockRepository.Object,
                _mockOrderServiceApi.Object,
                _mockLogger.Object
                );
        }

        [Theory, AutoProcessingOrderData]
        public async Task Should_CompleteAssembly_WhenProcessingOrderExists(ProcessingOrder processingOrder)
        {
            //Arrange
            var stopWatch = new Stopwatch();
            var expectedTime = TimeSpan.FromSeconds(30 * processingOrder.Items.Count);
            processingOrder.Status = ProcessingStatus.Processing;
            var initialUpdatedAt = processingOrder.UpdatedAt;
            _mockRepository
                .Setup(repo => repo.GetByIdAsync(
                    processingOrder.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((Guid id, CancellationToken ct) => processingOrder);

            _mockRepository
                .Setup(repo => repo.UpdateItemsAssemblyStatusAsync(
                    processingOrder.Id,
                    ItemAssemblyStatus.Ready,
                    It.IsAny<CancellationToken>()))
                .Callback(() => processingOrder.Items
                    .ForEach(i => i.Status = ItemAssemblyStatus.Ready));

            _mockRepository
                .Setup(repo => repo.UpdateAsync(
                    It.Is<ProcessingOrder>(po =>
                        po.Id == processingOrder.Id &&
                        po.Items.All(i => i.Status == ItemAssemblyStatus.Ready) &&
                        po.Stage == Stage.Assembly),
                    It.IsAny<CancellationToken>()));

            _mockOrderServiceApi
                .Setup(api => api.UpdateStatus(
                    processingOrder.OrderId,
                    "Ready",
                    It.IsAny<CancellationToken>()));

            //Act
            stopWatch.Start();
            await _asseblyWorker.ProcessAsync(processingOrder.Id, CancellationToken.None);
            stopWatch.Stop();

            //Assert
            _mockRepository.VerifyAll();
            _mockOrderServiceApi.VerifyAll();
            Assert.True(processingOrder.Items.All(i => i.Status == ItemAssemblyStatus.Ready));
            Assert.Equal(ProcessingStatus.Completed, processingOrder.Status);
            Assert.Equal(Stage.Assembly, processingOrder.Stage);
            Assert.True(initialUpdatedAt < processingOrder.UpdatedAt);
            stopWatch.Elapsed.Should().BeCloseTo(expectedTime, TimeSpan.FromMilliseconds(500));
        }

        [Fact]
        public async Task Should_CancelAssembly_WhenProcessingOrderNotFound()
        {
            //Arrange
            var nonExistentId = Guid.NewGuid();

            //Act
            await _asseblyWorker.ProcessAsync(nonExistentId, CancellationToken.None);

            //Assert
            _mockRepository
                .Verify(repo => repo.GetByIdAsync(
                    nonExistentId,
                    It.IsAny<CancellationToken>()),
                    Times.Once());

            _mockRepository
                .Verify(repo => repo.UpdateItemsAssemblyStatusAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<ItemAssemblyStatus>(),
                    It.IsAny<CancellationToken>()),
                    Times.Never());

            _mockRepository
                .Verify(repo => repo.UpdateAsync(
                    It.IsAny<ProcessingOrder>(),
                    It.IsAny<CancellationToken>()),
                    Times.Never());

            _mockOrderServiceApi
                .Verify(api => api.UpdateStatus(
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()),
                    Times.Never());

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, _) => v.ToString().Contains($"Processing order with ID {nonExistentId} not found")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
