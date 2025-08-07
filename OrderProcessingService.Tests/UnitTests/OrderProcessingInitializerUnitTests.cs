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
        private readonly Mock<IRepository<ProcessingOrder>> _mockRepository;
        private readonly OrderProcessingInitializer _orderProcessingInitializer;

        public OrderProcessingInitializerUnitTests() 
        {
            _mockRepository = new Mock<IRepository<ProcessingOrder>>();
            _orderProcessingInitializer = new OrderProcessingInitializer(_mockRepository.Object);
        }


        [Theory, AutoProcessingOrderData]
        public async Task Should_CreateProcessingOrderFromDtoAndSaveToDB(OrderDto dto)
        {
            //Arrange
            _mockRepository
                .Setup(repo => repo.CreateAsync(
                    It.Is<ProcessingOrder>(po =>
                        po.Id != Guid.Empty &&
                        po.Id != dto.Id &&
                        po.OrderId == dto.Id &&
                        po.Items.Count == dto.Items.Count &&
                        po.CreatedAt == dto.CreatedAt &&
                        po.UpdatedAt == dto.UpdatedAt &&
                        po.Status == ProcessingStatus.New &&
                        po.Stage == Stage.Assembly &&
                        po.TrackingNumber == null &&
                        po.Items.All(i => 
                            dto.Items.Any(dtoItem => 
                                dtoItem.ProductId == i.ProductId &&
                                dtoItem.Quantity == i.Quantity &&
                                i.Status == ItemAssemblyStatus.Pending))),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((ProcessingOrder po, CancellationToken cs) => po.Id);

            //Act
            await _orderProcessingInitializer.InitializeProcessingAsync(dto, CancellationToken.None);

            //Assert
            _mockRepository.VerifyAll();
        }

    }

}
