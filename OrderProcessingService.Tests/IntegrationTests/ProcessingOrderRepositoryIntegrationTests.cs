using AutoFixture;
using Castle.Components.DictionaryAdapter;
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

        private void AssertProcessingOrdersEquality(ProcessingOrder expected, ProcessingOrder actual, bool withItems = true)
        {
            Assert.NotNull(expected);
            Assert.NotNull(actual);
            Assert.Equal(expected.Id, actual.Id);
            Assert.Equal(expected.OrderId, actual.OrderId);
            Assert.Equal(expected.Status, actual.Status);
            Assert.Equal(expected.Stage, actual.Stage);
            Assert.Equal(expected.TrackingNumber, actual.TrackingNumber);
            Assert.Equal(expected.UpdatedAt, actual.UpdatedAt, TimeSpan.FromMilliseconds(5));
            Assert.Equal(expected.CreatedAt, actual.CreatedAt, TimeSpan.FromMilliseconds(5));
            if (withItems)
            {
                AssertOrderItemsEquality(expected.Items, actual.Items);
            }
            

        }

        private void AssertOrderItemsEquality(List<OrderItem> expectedItems, List<OrderItem> actualItems)
        {
            Assert.NotNull(expectedItems);
            Assert.NotNull(actualItems);
            Assert.Equal(expectedItems.Count, actualItems.Count);
            foreach (var expectedItem in expectedItems)
            {
                var actualItem = actualItems.First(i => i.ProductId == expectedItem.ProductId);
                Assert.NotNull(actualItem);
                Assert.Equal(expectedItem.Quantity, actualItem.Quantity);
                Assert.Equal(expectedItem.Status, actualItem.Status);
                Assert.Equal(expectedItem.ProcessingOrderId, actualItem.ProcessingOrderId);
                Assert.Equal(expectedItem.ProductId, actualItem.ProductId);
                Assert.Equal(expectedItem.Id, actualItem.Id);
            }
        }

        [Theory]
        [AutoProcessingOrderData]
        public async Task Should_ReturnProcessingOrderAndSaveToDB_WhenInsertingNewProcessingOrder(ProcessingOrder processingOrder)
        {
            //Arrange
            var expectedId = processingOrder.Id;

            //Act
            var actual = await _fixture.ProcessingOrdersRepository.InsertAsync(processingOrder, CancellationToken.None);
            await _fixture.ProcessingOrdersRepository.SaveChangesAsync(CancellationToken.None);
            _fixture.DbContext.ChangeTracker.Clear();

            //Assert
            Assert.Equal(expectedId, actual.Id);
            var savedProcessingOrder = await _fixture.DbContext.ProcessingOrders
                .Include(po => po.Items) 
                .AsNoTracking() 
                .FirstOrDefaultAsync(po => po.Id == expectedId);
            Assert.NotNull(savedProcessingOrder);
            AssertProcessingOrdersEquality(processingOrder, savedProcessingOrder);
        }

        [Theory]
        [AutoProcessingOrderData]
        public async Task Should_ReturnProcessingOrderWithoutItems_WhenExists(List<ProcessingOrder> processingOrders)
        {
            //Arrange
            await _fixture.DbContext.AddRangeAsync(processingOrders, CancellationToken.None);
            await _fixture.DbContext.SaveChangesAsync(CancellationToken.None);
            _fixture.DbContext.ChangeTracker.Clear();
            var expectedProcessingOrder = processingOrders.First();

            //Act
            var actualProcessingOrder = await _fixture.ProcessingOrdersRepository.GetByIdAsync(expectedProcessingOrder.Id, CancellationToken.None);

            //Assert
            Assert.NotNull(actualProcessingOrder);
            AssertProcessingOrdersEquality(expectedProcessingOrder, actualProcessingOrder, false);
        }

        [Theory]
        [AutoProcessingOrderData]
        public async Task Should_ReturnProcessingOrderIncludingItems_WhenExists(List<ProcessingOrder> processingOrders)
        {
            //Arrange
            await _fixture.DbContext.AddRangeAsync(processingOrders, CancellationToken.None);
            await _fixture.DbContext.SaveChangesAsync(CancellationToken.None);
            _fixture.DbContext.ChangeTracker.Clear();
            var expectedProcessingOrder = processingOrders.First();

            //Act
            var actualProcessingOrder = await _fixture.ProcessingOrdersRepository.GetFirstOrDefaultAsync(
                filter: (po) => po.Id == expectedProcessingOrder.Id,
                include: (q) => q.Include(po => po.Items),
                ct: CancellationToken.None);

            //Assert
            Assert.NotNull(actualProcessingOrder);
            AssertProcessingOrdersEquality(expectedProcessingOrder, actualProcessingOrder);
            Assert.Equal(expectedProcessingOrder.Id, actualProcessingOrder.Id);
        }

        [Fact]
        public async Task Should_ReturnNull_WhenProcessingOrderNotFound()
        {
            //Arange
            var expectedId = Guid.NewGuid();

            //Act
            var actualProcessingOrder = await _fixture.ProcessingOrdersRepository.GetByIdAsync(expectedId, CancellationToken.None);

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
            var affectedRows = await _fixture.ProcessingOrdersRepository.SaveChangesAsync(CancellationToken.None);
            _fixture.DbContext.ChangeTracker.Clear();

            //Assert
            Assert.Equal(1, affectedRows);
            var updatedProcessingOrder = await _fixture.DbContext.ProcessingOrders
                .Include(po => po.Items)
                .AsNoTracking()
                .FirstOrDefaultAsync(po => po.Id == processingOrder.Id);
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
            var newItemAssemblyStatus = ItemAssemblyStatus.Ready;

            //Act
            await _fixture.OrderItemsRepository.ExecuteUpdateAsync(
                setPropertyCalls: (call) => call.SetProperty((i) => i.Status, newItemAssemblyStatus), 
                filter: (item) => item.ProcessingOrderId == processingOrder.Id, 
                CancellationToken.None);

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
            var SearchIds = processingOrdersToFind.Select(po => po.Id).ToList();
            var expectedIds = new HashSet<Guid>(SearchIds);

            //Act
            var actualProcessinOrders = await _fixture.ProcessingOrdersRepository.GetAllAsync(
                filter: (po) => SearchIds.Contains(po.Id), 
                ct: CancellationToken.None);

            //Assert
            var actualIds = new HashSet<Guid>(actualProcessinOrders.Select(po => po.Id));
            Assert.True(expectedIds.SetEquals(actualIds));
            Assert.Equal(processingOrdersToFind.Count, actualProcessinOrders.Count);
            foreach (var actualProcessingOrder in actualProcessinOrders)
            {
                var expectedProcessingOrder = processingOrdersToFind.First(po => po.Id == actualProcessingOrder.Id);
                AssertProcessingOrdersEquality(expectedProcessingOrder, actualProcessingOrder, false);
            }
        }

        [Theory, AutoProcessingOrderData]
        public async Task Should_ReturnOnlyExistingProcessingOrders(ProcessingOrder processingOrder)
        {
            //Arrange
            await _fixture.DbContext.AddAsync(processingOrder, CancellationToken.None);
            await _fixture.DbContext.SaveChangesAsync(CancellationToken.None);
            _fixture.DbContext.ChangeTracker.Clear();
            var searchIds = new List<Guid>() { Guid.NewGuid(), processingOrder.Id };

            //Act
            var actualProcessingOrders = await _fixture.ProcessingOrdersRepository.GetAllAsync(
                filter: (po) => searchIds.Contains(po.Id),
                ct: CancellationToken.None);

            //Assert
            Assert.Single(actualProcessingOrders);
            AssertProcessingOrdersEquality(processingOrder, actualProcessingOrders[0], false);
        }


        [Theory, AutoProcessingOrderData]
        public async Task Should_AssignUniqueTrackingNumbersToProcessingOrders_WhenExecutingUpdate(List<ProcessingOrder> processingOrders)
        {
            //Arrange
            await _fixture.DbContext.AddRangeAsync(processingOrders, CancellationToken.None);
            await _fixture.DbContext.SaveChangesAsync(CancellationToken.None);
            _fixture.DbContext.ChangeTracker.Clear();
            var processingOrdersToUpdate = processingOrders.Skip(1).ToList();
            var searchIds = processingOrdersToUpdate.Select(po => po.Id).ToList();

            //Act
            var affectedRows = await _fixture.ProcessingOrdersRepository.ExecuteUpdateAsync(
                setPropertyCalls: (call) => call.SetProperty(po => po.TrackingNumber, Guid.NewGuid().ToString()), 
                filter: (po) => searchIds.Contains(po.Id),
                CancellationToken.None);

            //Assert
            Assert.Equal(processingOrdersToUpdate.Count, affectedRows);

            var updatedProcessingOrders = await _fixture.DbContext.ProcessingOrders
                .Where(po => searchIds.Contains(po.Id))
                .ToListAsync();

            Assert.All(updatedProcessingOrders, po =>
            {
                Assert.NotNull(po.TrackingNumber);
                Assert.True(Guid.TryParse(po.TrackingNumber, out _));
            });

            var trackingNumbers = updatedProcessingOrders
                .Select(po => po.TrackingNumber)
                .ToList();

            Assert.Equal(trackingNumbers.Count, new HashSet<string>(trackingNumbers).Count);

            var untouchedProcessingOrder = await _fixture.DbContext.ProcessingOrders.FindAsync(processingOrders[0].Id);
            Assert.Null(untouchedProcessingOrder!.TrackingNumber);
            Assert.Equal(processingOrders[0].UpdatedAt, untouchedProcessingOrder!.UpdatedAt, TimeSpan.FromMicroseconds(5));
        }
    }
}
