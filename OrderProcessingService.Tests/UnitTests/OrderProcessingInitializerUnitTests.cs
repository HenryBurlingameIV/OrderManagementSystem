using Castle.Core.Logging;
using Microsoft.Extensions.Logging;
using Moq;
using OrderManagementSystem.Shared.Contracts;
using OrderProcessingService.Application.DTO;
using OrderProcessingService.Application.Services;
using OrderProcessingService.Domain.Entities;
using OrderProcessingService.Tests.ProcessingOrderFixture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderProcessingService.Tests.UnitTests
{
    public class OrderProcessingInitializerUnitTests
    {
        private readonly Mock<IEFRepository<ProcessingOrder, Guid>> _mockRepository;
        private readonly Mock<ILogger<OrderProcessingInitializer>> _mockLogger;
        private readonly OrderProcessingInitializer _orderProcessingInitializer;

        public OrderProcessingInitializerUnitTests()
        {
            _mockRepository = new Mock<IEFRepository<ProcessingOrder, Guid>>();
            _mockLogger = new Mock<ILogger<OrderProcessingInitializer>>();
            _orderProcessingInitializer = new OrderProcessingInitializer(_mockRepository.Object, _mockLogger.Object);
        }


        [Theory, AutoProcessingOrderData]
        public async Task Should_CreateProcessingOrderFromDtoAndSaveToDB(OrderDto dto)
        {
            //Arrange
            ProcessingOrder capturedOrder = null;

            _mockRepository
                .Setup(repo => repo.InsertAsync(It.IsAny<ProcessingOrder>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((ProcessingOrder po, CancellationToken ct) =>
                {
                    capturedOrder = po;
                    return po;
                });

            //Act
            await _orderProcessingInitializer.InitializeProcessingAsync(dto, CancellationToken.None);

            //Assert
            Assert.Equal(dto.Id, capturedOrder?.OrderId);
            Assert.Equal(dto.Items.Count, capturedOrder?.Items.Count);
            Assert.Equal(dto.CreatedAt, capturedOrder?.CreatedAt);
            _mockRepository.Verify(repo => repo.InsertAsync(It.IsAny<ProcessingOrder>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockRepository.Verify(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

    }

}
