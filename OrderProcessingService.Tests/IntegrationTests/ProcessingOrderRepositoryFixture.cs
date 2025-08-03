using Microsoft.EntityFrameworkCore;
using OrderProcessingService.Infrastructure;
using OrderProcessingService.Infrastructure.Repositories;
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
        public ProcessingOrderRepository processingOrderRepository { get; private set; }

        public async Task InitializeAsync()
        {
            var options = new DbContextOptionsBuilder()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            DbContext = new OrderProcessingDbContext(options);

            processingOrderRepository = new ProcessingOrderRepository(DbContext);
            await DbContext.Database.EnsureCreatedAsync();
        }

        public async Task DisposeAsync()
        {
            await DbContext.DisposeAsync();
        }

        public async Task ResetDataBase()
        {
            await DbContext.Database.EnsureDeletedAsync();
            DbContext.ChangeTracker.Clear();
        }

    }
}
