using Castle.Core.Logging;
using Microsoft.Extensions.Logging;
using Moq;
using OrderManagementSystem.Shared.Exceptions;
using OrderProcessingService.Application.Contracts;
using OrderProcessingService.Application.DTO;
using OrderProcessingService.Application.Services;
using OrderProcessingService.Application.Validators;
using OrderProcessingService.Domain.Entities;
using OrderProcessingService.Tests.ProcessingOrderFixture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderProcessingService.Tests.UnitTests
{
    public class OrderProcessorUnitTests
    {
        private readonly Mock<IProcessingOrderRepository> _mockRepository;
        private readonly Mock<IOrderBackgroundWorker<StartAssemblyCommand>> _mockAssemblyWorker;
        private readonly Mock<IOrderServiceApi> _mockOrderServiceApi;
        private readonly Mock<ILogger<OrderProcessor>> _mockLogger;
        private readonly Mock<IOrderBackgroundWorker<StartDeliveryCommand>> _mockDeliveryWorker;
        private readonly StartAssemblyValidator _assemblyStatusValidator;
        private readonly StartDeliveryValidator _deliveryStatusValidator;
        private readonly OrderProcessor _orderProcessor;

        public OrderProcessorUnitTests() 
        {
            _mockRepository = new Mock<IProcessingOrderRepository>();
            _mockAssemblyWorker = new Mock<IOrderBackgroundWorker<StartAssemblyCommand>>();
            _mockOrderServiceApi = new Mock<IOrderServiceApi>();
            _mockLogger = new Mock<ILogger<OrderProcessor>>();
            _mockDeliveryWorker = new Mock<IOrderBackgroundWorker<StartDeliveryCommand>>();
            _assemblyStatusValidator = new StartAssemblyValidator();
            _deliveryStatusValidator = new StartDeliveryValidator();
            _orderProcessor = new OrderProcessor(
                _mockRepository.Object,
                _mockAssemblyWorker.Object,
                _mockDeliveryWorker.Object,
                _mockOrderServiceApi.Object,
                _mockLogger.Object,
                _assemblyStatusValidator,
                _deliveryStatusValidator
                );
        }

        [Theory, AutoProcessingOrderData]
        public async Task Should_BeginAssemblyAndUpdateOrder_WhenProcessingOrderExists(ProcessingOrder processingOrder)
        {
            //Arrange
            var poId = processingOrder.Id;
            var orderId = processingOrder.OrderId;
            var initialUpdatedAt = processingOrder.UpdatedAt;
            _mockRepository
                .Setup(repo => repo.GetByIdAsync(poId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Guid id, CancellationToken ct) => processingOrder);

            _mockRepository
               .Setup(repo => repo.UpdateAsync(
                    It.Is<ProcessingOrder>(po => 
                        po.Id == poId &&
                        po.OrderId == orderId &&
                        po.Items.Count == processingOrder.Items.Count &&
                        po.Status == ProcessingStatus.Processing &&
                        po.Stage == Stage.Assembly &&
                        po.UpdatedAt > initialUpdatedAt),
                    It.IsAny<CancellationToken>()))
               .ReturnsAsync((ProcessingOrder processingOrder, CancellationToken ct) => processingOrder.Id);

            _mockAssemblyWorker
                .Setup(worker => worker.ScheduleAsync(
                    It.Is<StartAssemblyCommand>(c => c.ProcessingOrderId == poId),
                    It.IsAny<CancellationToken>()));

            _mockOrderServiceApi
                .Setup(api => api.UpdateStatus(orderId, "Processing", It.IsAny<CancellationToken>()));

            //Act
            await _orderProcessor.BeginAssembly(poId, CancellationToken.None);

            //Assert
            _mockRepository.VerifyAll();
            _mockAssemblyWorker.VerifyAll();
            _mockOrderServiceApi.VerifyAll();

        }


        [Fact]
        public async Task Should_ThrowNotFoundException_WhenProcessingOrderNotFound()
        {
            //Arange
            var nonExistentId = Guid.NewGuid();
            _mockRepository.Setup(x => x.GetByIdAsync(nonExistentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((ProcessingOrder)null);
            //Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
                _orderProcessor.BeginAssembly(nonExistentId, CancellationToken.None));
            Assert.Contains($"Processing order with ID {nonExistentId} not found.", exception.Message);
        }

    }
}
