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

            Assert.Equal(expectedItems.Count, savedProcessingOrder.Items.Count);

            foreach(var expectedItem in expectedItems)
            {
                var actualItem = savedProcessingOrder.Items.First(i => i.ProductId == expectedItem.ProductId);
                Assert.Equal(expectedItem.Status, actualItem.Status);
                Assert.Equal(expectedItem.Quantity, actualItem.Quantity);
            }
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
            Assert.Equal(expectedProcessingOrder.Items.Count, actualProcessingOrder.Items.Count);

            foreach (var expectedItem in expectedItems)
            {
                var actualItem = actualProcessingOrder.Items.First(i => i.ProductId == expectedItem.ProductId);
                Assert.Equal(expectedItem.Status, actualItem.Status);
                Assert.Equal(expectedItem.Quantity, actualItem.Quantity);
            }
        }
    }
}
