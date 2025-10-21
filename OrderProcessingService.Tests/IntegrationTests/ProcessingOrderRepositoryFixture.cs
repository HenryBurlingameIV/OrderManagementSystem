using Microsoft.EntityFrameworkCore;
using OrderManagementSystem.Shared.Contracts;
using OrderManagementSystem.Shared.DataAccess;
using OrderProcessingService.Domain.Entities;
using OrderProcessingService.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderProcessingService.Tests.IntegrationTests
{
    public class ProcessingOrderRepositoryFixture : IAsyncLifetime
    {
        public OrderProcessingDbContext DbContext { get; private set; }
        public IEFRepository<ProcessingOrder,Guid> ProcessingOrdersRepository { get; private set; }

        public IEFRepository<OrderItem, Guid> OrderItemsRepository { get; private set; }

        public async Task InitializeAsync()
        {
            var options = new DbContextOptionsBuilder()
                .UseNpgsql("Host=localhost;Database=order_processing_test_db;Port=5432;Username=postgres;Password=0807")
                .Options;

            DbContext = new OrderProcessingDbContext(options);

            ProcessingOrdersRepository = new Repository<ProcessingOrder, Guid>(DbContext);
            OrderItemsRepository = new Repository<OrderItem, Guid>(DbContext);
            await DbContext.Database.EnsureCreatedAsync();
        }

        public async Task DisposeAsync()
        {
            await DbContext.Database.EnsureDeletedAsync();
            await DbContext.DisposeAsync();
        }

        public async Task ResetDataBase()
        {
            await DbContext.Database.EnsureDeletedAsync();
            await DbContext.Database.EnsureCreatedAsync();
            DbContext.ChangeTracker.Clear();
        }

    }
}
