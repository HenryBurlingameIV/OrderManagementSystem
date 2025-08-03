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

        private void AssertProcessingOrdersEquality(ProcessingOrder expectedProcessingOrder, ProcessingOrder actualProcessingOrder)
        {
            Assert.Equal(expectedProcessingOrder.Id, actualProcessingOrder.Id);
            Assert.Equal(expectedProcessingOrder.OrderId, actualProcessingOrder.OrderId);
            Assert.Equal(expectedProcessingOrder.Status, actualProcessingOrder.Status);
            Assert.Equal(expectedProcessingOrder.Stage, actualProcessingOrder.Stage);
            Assert.Equal(expectedProcessingOrder.TrackingNumber, actualProcessingOrder.TrackingNumber);
            Assert.Equal(expectedProcessingOrder.UpdatedAt, actualProcessingOrder.UpdatedAt, TimeSpan.FromMilliseconds(5));
            Assert.Equal(expectedProcessingOrder.CreatedAt, actualProcessingOrder.CreatedAt, TimeSpan.FromMilliseconds(5));
            AssertOrderItemsEquality(expectedProcessingOrder.Items, actualProcessingOrder.Items);

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
            AssertProcessingOrdersEquality(processingOrder, savedProcessingOrder);
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

            //Act
            var actualProcessingOrder = await _fixture.ProcessingOrderRepository.GetByIdAsync(expectedProcessingOrder.Id, CancellationToken.None);

            //Assert
            Assert.NotNull(actualProcessingOrder);
            AssertProcessingOrdersEquality(expectedProcessingOrder, actualProcessingOrder);
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
            Assert.Equal(newUpdatedAt, updatedProcessingOrder.UpdatedAt, TimeSpan.FromMicroseconds(5));
            Assert.Equal(newStage, updatedProcessingOrder.Stage);
            Assert.Equal(processingOrder.CreatedAt, updatedProcessingOrder.CreatedAt, TimeSpan.FromMicroseconds(5));
            AssertOrderItemsEquality(processingOrder.Items, updatedProcessingOrder.Items);
        }

        [Theory, AutoProcessingOrderData]
        public async Task Should_UpdateAllItemsAssemblyStatusInOrder_WhenProcessingOrderExists(ProcessingOrder processingOrder)
        {
            //Arrange
            await _fixture.DbContext.ProcessingOrders.AddAsync(processingOrder, CancellationToken.None);
            await _fixture.DbContext.SaveChangesAsync(CancellationToken.None);
            _fixture.DbContext.ChangeTracker.Clear();

            //Act
            await _fixture.ProcessingOrderRepository.UpdateItemsAssemblyStatusAsync(processingOrder.Id, ItemAssemblyStatus.Ready, CancellationToken.None);

            //Assert
            var updatedProcessingOrder = await _fixture.DbContext.ProcessingOrders
               .Include(po => po.Items)
               .AsNoTracking()
               .FirstOrDefaultAsync(po => po.Id == processingOrder.Id);
            Assert.NotNull(updatedProcessingOrder);
            Assert.All(updatedProcessingOrder!.Items, i =>
            {
                Assert.Equal(ItemAssemblyStatus.Ready, i.Status);
            });
        }

        [Theory, AutoProcessingOrderData]
        public async Task Should_ReturnProcessingOrdersWithRequiredIds(List<ProcessingOrder> processingOrders)
        {
            //Arrange
            await _fixture.DbContext.AddRangeAsync(processingOrders, CancellationToken.None);
            await _fixture.DbContext.SaveChangesAsync(CancellationToken.None);
            _fixture.DbContext.ChangeTracker.Clear();
            var processingOrdersToFind = processingOrders.Skip(1).ToList();
            var idsToFind = processingOrdersToFind.Select(po => po.Id).ToList();
            var expectedIds = new HashSet<Guid>(idsToFind);

            //Act
            var actualProcessinOrders = await _fixture.ProcessingOrderRepository.GetByIdsAsync(idsToFind, CancellationToken.None);

            //Assert
            var actualIds = new HashSet<Guid>(actualProcessinOrders.Select(po => po.Id));
            Assert.True(expectedIds.SetEquals(actualIds));
            Assert.Equal(processingOrdersToFind.Count, actualProcessinOrders.Count);
            foreach(var actualProcessingOrder in  actualProcessinOrders)
            {
                var expectedProcessingOrder = processingOrdersToFind.First(po => po.Id == actualProcessingOrder.Id);
                AssertProcessingOrdersEquality(expectedProcessingOrder, actualProcessingOrder);
            }
        }

        [Theory, AutoProcessingOrderData]
        public async Task Should_ReturnOnlyExistingProcessingOrders(ProcessingOrder processingOrder)
        {
            //Arrange
            await _fixture.DbContext.AddAsync(processingOrder, CancellationToken.None);
            await _fixture.DbContext.SaveChangesAsync(CancellationToken.None);
            _fixture.DbContext.ChangeTracker.Clear();
            var idsToFins = new List<Guid>() { Guid.NewGuid() , processingOrder.Id};

            //Act
            var actualProcessingOrders = await _fixture.ProcessingOrderRepository.GetByIdsAsync(idsToFins, CancellationToken.None);

            //Assert
            Assert.Single(actualProcessingOrders);
            AssertProcessingOrdersEquality(processingOrder, actualProcessingOrders[0]);
        }


        [Theory, AutoProcessingOrderData]
        public async Task Should_AssignUniqueTrackingNumbersToProcessingOrdersWithRequiredIds(List<ProcessingOrder> processingOrders)
        {
            //Arrange
            await _fixture.DbContext.AddRangeAsync(processingOrders, CancellationToken.None);
            await _fixture.DbContext.SaveChangesAsync(CancellationToken.None);
            _fixture.DbContext.ChangeTracker.Clear();
            var processingOrdersToUpdate = processingOrders.Skip(1).ToList();
            var idsToFind = processingOrdersToUpdate.Select(po => po.Id).ToList();

            //Act
            await _fixture.ProcessingOrderRepository.AssignUniqueTrackingNumbersAsync(idsToFind, CancellationToken.None);

            //Assert
            var updatedProcessingOrders = await _fixture.DbContext.ProcessingOrders
                .Where(po => idsToFind.Contains(po.Id))
                .ToListAsync();

            Assert.All(updatedProcessingOrders, po =>
            {
                Assert.NotNull(po.TrackingNumber);
                Assert.True(Guid.TryParse(po.TrackingNumber, out _));
            });
            Assert.True(processingOrdersToUpdate[0].UpdatedAt < updatedProcessingOrders[0].UpdatedAt);

            var trackingNumbers = updatedProcessingOrders
                .Select(po => po.TrackingNumber)
                .ToList();

            Assert.Equal(trackingNumbers.Count, new HashSet<string>(trackingNumbers).Count);

            var notUpdatedProcessingOrder = await _fixture.DbContext.ProcessingOrders.FindAsync(processingOrders[0].Id);
            Assert.Null(notUpdatedProcessingOrder!.TrackingNumber);
            Assert.Equal(processingOrders[0].UpdatedAt, notUpdatedProcessingOrder!.UpdatedAt, TimeSpan.FromMicroseconds(5));
        }
    }
}
