using Castle.Core.Logging;
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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using static Dapper.SqlMapper;

namespace OrderProcessingService.Tests.UnitTests
{
    public class AssemblyWorkerUnitTests
    {
        private readonly Mock<IEFRepository<ProcessingOrder, Guid>> _mockProcessingOrdersRepository;
        private readonly Mock<IEFRepository<OrderItem, Guid>> _mockOrderItemsRepository;
        private readonly Mock<IBackgroundJobClient> _mockBackgroundJobClient;
        private readonly Mock<IOrderServiceApi> _mockOrderServiceApi;
        private readonly Mock<ILogger<AssemblyWorker>> _mockLogger;
        private readonly AssemblyWorker _asseblyWorker;

        public AssemblyWorkerUnitTests()
        {
            _mockProcessingOrdersRepository = new Mock<IEFRepository<ProcessingOrder, Guid>>();
            _mockOrderItemsRepository = new Mock<IEFRepository<OrderItem, Guid>>();
            _mockBackgroundJobClient = new Mock<IBackgroundJobClient>();
            _mockOrderServiceApi = new Mock<IOrderServiceApi>();
            _mockLogger = new Mock<ILogger<AssemblyWorker>>();
            _asseblyWorker = new AssemblyWorker(
                _mockBackgroundJobClient.Object,
                _mockProcessingOrdersRepository.Object,
                _mockOrderItemsRepository.Object,
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
            _mockProcessingOrdersRepository
                .Setup(repo => repo.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<ProcessingOrder, bool>>>(),
                    It.IsAny<Func<IQueryable<ProcessingOrder>, IIncludableQueryable<ProcessingOrder, object>>>(),
                    null,
                    false,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(processingOrder);

            _mockOrderItemsRepository
                .Setup(repo => repo.ExecuteUpdateAsync(
                    It.IsAny<Expression<Func<SetPropertyCalls<OrderItem>, SetPropertyCalls<OrderItem>>>>(),
                    It.IsAny<Expression<Func<OrderItem, bool>>>(),
                    It.IsAny<CancellationToken>()))
                .Callback(() => processingOrder.Items
                    .ForEach(i => i.Status = ItemAssemblyStatus.Ready));

            _mockProcessingOrdersRepository
                .Setup(repo => repo.SaveChangesAsync(
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
            _mockProcessingOrdersRepository.VerifyAll();
            _mockOrderItemsRepository.VerifyAll();
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
            _mockProcessingOrdersRepository
                .Verify(repo => repo.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<ProcessingOrder, bool>>>(),
                    It.IsAny<Func<IQueryable<ProcessingOrder>, IIncludableQueryable<ProcessingOrder, object>>>(),
                    null,
                    false,
                    It.IsAny<CancellationToken>()),
                    Times.Once());

            _mockOrderItemsRepository
                .Verify(repo => repo.ExecuteUpdateAsync(
                    It.IsAny<Expression<Func<SetPropertyCalls<OrderItem>, SetPropertyCalls<OrderItem>>>>(),
                    It.IsAny<Expression<Func<OrderItem, bool>>>(),
                    It.IsAny<CancellationToken>()),
                    Times.Never());

            _mockProcessingOrdersRepository
                .Verify(repo => repo.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
                    Times.Never);

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
