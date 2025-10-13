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
        private readonly OrderProcessingInitializer _orderProcessingInitializer;

        public OrderProcessingInitializerUnitTests()
        {
            _mockRepository = new Mock<IEFRepository<ProcessingOrder, Guid>>();
            _orderProcessingInitializer = new OrderProcessingInitializer(_mockRepository.Object);
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
            _mockRepository.Verify(repo => repo.InsertAsync(It.IsAny<ProcessingOrder>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockRepository.Verify(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

    }

}
