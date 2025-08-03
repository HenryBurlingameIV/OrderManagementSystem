using AutoFixture;
using Microsoft.EntityFrameworkCore;
using OrderProcessingService.Application.Contracts;
using OrderProcessingService.Domain.Entities;
using OrderProcessingService.Tests.ProcessingOrderFixture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderProcessingService.Tests.IntegrationTests
{
    public class ProcessingOrderRepositoryIntegrationTests : IClassFixture<ProcessingOrderRepositoryFixture>, IAsyncLifetime
    {
        private readonly ProcessingOrderRepositoryFixture _fixture;

        public ProcessingOrderRepositoryIntegrationTests(ProcessingOrderRepositoryFixture fixture)
        {
            _fixture = fixture;
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

        public async Task InitializeAsync()
        {
            await _fixture.ResetDataBase();
        }

        private void AssertOrderItemsEquality(List<OrderItem> expectedItems, List<OrderItem> actualItems)
        {
            Assert.Equal(expectedItems.Count, actualItems.Count);
            foreach (var expectedItem in expectedItems)
            {
                var actualItem = actualItems.First(i => i.ProductId == expectedItem.ProductId);
                Assert.Equal(expectedItem.Quantity, actualItem.Quantity);
                Assert.Equal(expectedItem.Status, actualItem.Status);
            }
        }

        [Theory]
        [AutoProcessingOrderData]
        public async Task Should_ReturnProcessingOrderIdAndSaveToDB_WhenCreatingNewProcessingOrder(ProcessingOrder processingOrder)
        {
            //Arrange
            var expectedId = processingOrder.Id;
            var expectedItems = processingOrder.Items.ToList();

            //Act
            var actualId = await _fixture.ProcessingOrderRepository.CreateAsync(processingOrder, CancellationToken.None);
            _fixture.DbContext.ChangeTracker.Clear();

            //Assert
            Assert.Equal(expectedId, actualId);
            var savedProcessingOrder = await _fixture.DbContext.ProcessingOrders
                .Include(po => po.Items) 
                .AsNoTracking() 
                .FirstOrDefaultAsync(po => po.Id == expectedId);
            Assert.NotNull(savedProcessingOrder);
            Assert.Equivalent(processingOrder, savedProcessingOrder);
            AssertOrderItemsEquality(expectedItems, savedProcessingOrder.Items);
        }

        [Theory]
        [AutoProcessingOrderData]
        public async Task Should_ReturnProcessingOrder_WhenExists(List<ProcessingOrder> processingOrders)
        {
            //Arrange
            await _fixture.DbContext.AddRangeAsync(processingOrders, CancellationToken.None);
            await _fixture.DbContext.SaveChangesAsync(CancellationToken.None);
            _fixture.DbContext.ChangeTracker.Clear();
            var expectedProcessingOrder = processingOrders.First();
            var expectedItems = expectedProcessingOrder.Items;

            //Act
            var actualProcessingOrder = await _fixture.ProcessingOrderRepository.GetByIdAsync(expectedProcessingOrder.Id, CancellationToken.None);

            //Assert
            Assert.NotNull(actualProcessingOrder);
            Assert.Equivalent(expectedProcessingOrder, actualProcessingOrder);
            AssertOrderItemsEquality(expectedItems, actualProcessingOrder.Items);
        }

        [Fact]
        public async Task Should_ReturnNull_WhenProcessingOrderNotFound()
        {
            //Arange
            var expectedId = Guid.NewGuid();

            //Act
            var actualProcessingOrder = await _fixture.ProcessingOrderRepository.GetByIdAsync(expectedId, CancellationToken.None);

            //Assert
            Assert.Null(actualProcessingOrder);
        }

        [Theory, AutoProcessingOrderData]
        public async Task Should_UpdateProcessingAndReturnId_WhenExists(ProcessingOrder processingOrder)
        {
            //Arrange
            processingOrder.Stage = Stage.Assembly;
            processingOrder.Status = ProcessingStatus.New;
            await _fixture.DbContext.AddAsync(processingOrder, CancellationToken.None);
            await _fixture.DbContext.SaveChangesAsync(CancellationToken.None);
            _fixture.DbContext.ChangeTracker.Clear();
            var newUpdatedAt = processingOrder.UpdatedAt;
            var newStage = Stage.Delivery;
            var newStatus = ProcessingStatus.Completed;
            var processingOrderToUpdate = await _fixture.DbContext.ProcessingOrders.FindAsync(processingOrder.Id);

            //Act
            processingOrderToUpdate!.UpdatedAt = newUpdatedAt;
            processingOrderToUpdate.Stage = newStage;
            processingOrderToUpdate.Status = newStatus;
            var actualId = await _fixture.ProcessingOrderRepository.UpdateAsync(processingOrderToUpdate, CancellationToken.None);
            _fixture.DbContext.ChangeTracker.Clear();

            //Assert
            Assert.Equal(processingOrder.Id, actualId);
            var updatedProcessingOrder = await _fixture.DbContext.ProcessingOrders
                .Include(po => po.Items)
                .AsNoTracking()
                .FirstOrDefaultAsync(po => po.Id == actualId);
            Assert.NotNull(updatedProcessingOrder);
            Assert.Equal(newUpdatedAt, updatedProcessingOrder.UpdatedAt);
            Assert.Equal(newStage, updatedProcessingOrder.Stage);
            Assert.Equal(processingOrder.CreatedAt, updatedProcessingOrder.CreatedAt);
            AssertOrderItemsEquality(processingOrder.Items, updatedProcessingOrder.Items);
        }
    }
}
